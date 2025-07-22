using Microsoft.AspNetCore.Identity;
using taskflow_api.TaskFlow.Application.DTOs.Request;
using taskflow_api.TaskFlow.Application.DTOs.Response;
using taskflow_api.TaskFlow.Application.Interfaces;
using taskflow_api.TaskFlow.Domain.Common.Enums;
using taskflow_api.TaskFlow.Domain.Entities;
using taskflow_api.TaskFlow.Infrastructure.Interfaces;
using taskflow_api.TaskFlow.Shared.Exceptions;
using taskflow_api.TaskFlow.Shared.Helpers;

namespace taskflow_api.TaskFlow.Application.Services
{
    public class MemberService : IMemberService
    {
        private readonly UserManager<User> _userManager;
        private readonly IVerifyTokenRopository _verifyTokenRopository;
        private readonly IProjectRepository _projectRepository;
        private readonly IProjectMemberRepository _projectMemberRepository;
        private readonly IMailService _mailService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITagRepository _TagRepository;
        private readonly INotificationService _notificationService;

        public MemberService(UserManager<User> userManager, IVerifyTokenRopository verifyTokenRopository,
                            IProjectRepository projectRepository, IProjectMemberRepository projectMemberRepository,
                            IMailService mailService, IHttpContextAccessor httpContextAccessor,
                            ITagRepository TagRepository, INotificationService notificationService)
        {
            _userManager = userManager;
            _verifyTokenRopository = verifyTokenRopository;
            _projectRepository = projectRepository;
            _projectMemberRepository = projectMemberRepository;
            _mailService = mailService;
            _httpContextAccessor = httpContextAccessor;
            _TagRepository = TagRepository;
            _notificationService = notificationService;
        }
        public async Task<bool> AddMember(Guid ProjectId, AddMemberRequest request)
        {
            var user = _userManager.FindByEmailAsync(request.Email);
            if (user.Result == null)
                throw new AppException(ErrorCode.NoUserFound);

            //Find the number of projects participated
            bool projectCount = await _projectMemberRepository.GetUserIsActiveInProjectAsync(user.Result.Id);
            if (projectCount)
            {
                throw new AppException(ErrorCode.MaxProjectLimitReached);
            }
            //save token to the database
            var token = GenerateRandom.GenerateRandomToken();
            await _verifyTokenRopository.AddVerifyTokenAsync(new VerifyToken
            {
                UserId = user.Result.Id,
                ProjectId = ProjectId,
                Token = token,
                Type = VerifyTokenEnum.JoinProject,
                IsUsed = false,
                ExpiresAt = DateTime.UtcNow.AddDays(3),
            });
            //check member has been in the project
            var member = _projectMemberRepository.FindMemberInProject(ProjectId, user.Result.Id);
            if (member.Result != null && member.Result.HasJoinedBefore)
            {
                //member back to the project
                //send email to the user
                await _mailService.SendMailJoinProject(request.Email, ProjectId, token, "come back to the project");
                await _notificationService.NotifyProjectMemberChangeAsync(ProjectId, $"Member {(string.IsNullOrEmpty(user.Result.FullName) ? user.Result.Email : user.Result.FullName)} has rejoined the project.");
                return true;
            }
            //create new member
            var projectMember = new ProjectMember
            {
                Id = Guid.NewGuid(),
                UserId = user.Result.Id,
                ProjectId = ProjectId,
                Role = ProjectRole.Member,
                IsActive = false
            };
            await _projectMemberRepository.CreateProjectMemeberAsync(projectMember);
            await _mailService.SendMailJoinProject(request.Email, ProjectId, token, "join the project");
            await _notificationService.NotifyProjectMemberChangeAsync(ProjectId, $"Member {(string.IsNullOrEmpty(user.Result.FullName) ? user.Result.Email : user.Result.FullName)} has been added to the project.");
            return true;
        }

        public Task AddSystemUSer(Guid ProjectId)
        {
            var systemUser = new ProjectMember
            {
                Id = Guid.NewGuid(),
                UserId = Guid.Parse("00000000-0000-0000-0000-000000000001"), // System user ID
                ProjectId = ProjectId,
                Role = ProjectRole.System,
                IsActive = true,
                HasJoinedBefore = true // Mark as has joined before
            };
            return _projectMemberRepository.CreateProjectMemeberAsync(systemUser);
        }

        public async Task<List<MemberResponse>> GetAllMemberInProject(Guid projectId)
        {
            return await _projectMemberRepository.GetAllMembersInProjectAsync(projectId);
        }

        public Task<ProjectMemberResponse?> GetMeInProject(Guid ProjectId, Guid ProjectMemberId)
        {
            return _projectMemberRepository.GetMeInProjectAsync(ProjectId, ProjectMemberId);
        }

        public async Task<bool> LeaveTheProject(Guid projectId)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var userId = httpContext?.User.FindFirst("id")?.Value;
            if (userId == null)
            {
                throw new AppException(ErrorCode.NoUserFound);
            }
            var member = await _projectMemberRepository.FindMemberInProject(projectId, Guid.Parse(userId));
            if (member == null)
            {
                throw new AppException(ErrorCode.NoUserFound);
            }

            //If you are the leader, only one member of the project can leave
            var countMembers = await _projectMemberRepository.GetActiveMembersCount(projectId);
            if (member.Role == ProjectRole.Leader && countMembers > 1)
            {
                throw new AppException(ErrorCode.CannotLeaveProjectAsPM);
            }
            //Remove the member from the project
            member.IsActive = false;
            await _projectMemberRepository.UpdateMember(member);
            return true;
        }
        public async Task<bool> RemoveMember(Guid projectId, Guid projectMemberId)
        {
            var member = await _projectMemberRepository.FindMemberInProjectByProjectMemberID(projectMemberId);
            if (member == null)
            {
                throw new AppException(ErrorCode.NoUserFound);
            }
            //Remove the member from the project
            member!.IsActive = false;
            member.HasJoinedBefore = true; // Mark as has joined before
            await _projectMemberRepository.UpdateMember(member);
            await _notificationService.NotifyProjectMemberChangeAsync(projectId, $"Member {(string.IsNullOrEmpty(member.User?.FullName) ? member.User?.Email : member.User?.FullName) ?? member.UserId.ToString()} has been removed from the project.");
            return true;
        }
        public async Task<bool> VerifyJoinProject(string token)
        {
            var verifyToken = await _verifyTokenRopository.GetVerifyTokenAsync(token);
            if (verifyToken == null || verifyToken.IsUsed || verifyToken.IsExpired)
            {
                throw new AppException(ErrorCode.InvalidToken);
            }
            var user = await _userManager.FindByIdAsync(verifyToken.UserId!.ToString());
            var memberProject = await _projectMemberRepository.FindMemberInProject(verifyToken.ProjectId!.Value, verifyToken.UserId);
            if (memberProject == null)
            {
                throw new AppException(ErrorCode.NoUserFound);
            }
            //Active the user in the project
            memberProject.IsActive = true;
            await _projectMemberRepository.UpdateMember(memberProject);

            //Update token
            verifyToken.IsUsed = true;
            await _verifyTokenRopository.UpdateTokenAsync(verifyToken);
            //create default Tags for the user
            var Tag = new Tag
            {
                Name = user!.FullName,
                ProjectId = verifyToken.ProjectId!.Value,
                Description = "Tag of "+user!.FullName,
                Color = "#FF5733", // Default color
            };
            await _TagRepository.AddTagAsync(Tag);
            return true;
        }

    }
}
