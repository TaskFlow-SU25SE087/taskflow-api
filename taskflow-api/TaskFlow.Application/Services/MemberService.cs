using Microsoft.AspNetCore.Identity;
using taskflow_api.TaskFlow.Application.DTOs.Request;
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
        private readonly ILabelRepository _labelRepository;

        public MemberService(UserManager<User> userManager, IVerifyTokenRopository verifyTokenRopository,
                            IProjectRepository projectRepository, IProjectMemberRepository projectMemberRepository,
                            IMailService mailService, IHttpContextAccessor httpContextAccessor,
                            ILabelRepository labelRepository)
        {
            _userManager = userManager;
            _verifyTokenRopository = verifyTokenRopository;
            _projectRepository = projectRepository;
            _projectMemberRepository = projectMemberRepository;
            _mailService = mailService;
            _httpContextAccessor = httpContextAccessor;
            _labelRepository = labelRepository;
        }
        public async Task<bool> AddMember(AddMemberRequest request)
        {
            var user = _userManager.FindByEmailAsync(request.Email);
            if (user.Result == null)
                throw new AppException(ErrorCode.NoUserFound);
            //save token to the database
            var token = GenerateRandom.GenerateRandomToken();
            await _verifyTokenRopository.AddVerifyTokenAsync(new VerifyToken
            {
                UserId = user.Result.Id,
                ProjectId = request.ProjectId,
                Token = token,
                Type = VerifyTokenEnum.JoinProject,
                IsUsed = false,
                ExpiresAt = DateTime.UtcNow.AddDays(3),
            });
            //check member has been in the project
            var member = _projectMemberRepository.FindMemberInProject(request.ProjectId, user.Result.Id);
            if (member.Result != null)
            {
                //member back to the project
                //send email to the user
                await _mailService.SendMailJoinProject(request.Email, token, "come back to the project");
                return true;
            }
            //create new member
            var projectMember = new ProjectMember
            {
                Id = Guid.NewGuid(),
                UserId = user.Result.Id,
                ProjectId = request.ProjectId,
                Role = ProjectRole.Member,
                IsActive = false
            };
            await _projectMemberRepository.CreateProjectMemeberAsync(projectMember);
            await _mailService.SendMailJoinProject(request.Email, token, "join the project");
            return true;
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
            //Check if the user is PM
            if (member.Role == ProjectRole.PM)
            {
                throw new AppException(ErrorCode.CannotLeaveProjectAsPM);
            }
            //Remove the member from the project
            member.IsActive = false;
            await _projectMemberRepository.UpdateMember(member);
            return true;
        }
        public async Task<bool> RemoveMember(Guid projectId, Guid userId)
        {
            var member = await _projectMemberRepository.FindMemberInProject(projectId, userId);
            if (member == null)
            {
                throw new AppException(ErrorCode.NoUserFound);
            }
            //Remove the member from the project
            member!.IsActive = false;
            await _projectMemberRepository.UpdateMember(member);
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
            //create default labels for the user
            var label = new Labels
            {
                Name = user!.FullName,
                ProjectId = verifyToken.ProjectId!.Value,
                Description = "Label of "+user!.FullName,
            };
            await _labelRepository.AddLabelAsync(label);
            return true;
        }

    }
}
