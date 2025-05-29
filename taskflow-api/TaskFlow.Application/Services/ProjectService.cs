using AutoMapper;
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
    public class ProjectService : IProjectService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IProjectRepository _projectRepository;
        private readonly IProjectMemberRepository _projectMemberRepository;
        private readonly IBoardRepository _boardRepository;
        private readonly IMailService _mailService;
        private readonly IMapper _mapper;
        private readonly IVerifyTokenRopository _verifyTokenRopository;

        public ProjectService(UserManager<User> userManager, SignInManager<User> signInManager,
            IHttpContextAccessor httpContextAccessor, IProjectRepository projectRepository,
            IProjectMemberRepository projectMember, IBoardRepository boardRepository,
            IMailService mailService, IMapper mapper, IVerifyTokenRopository verifyTokenRopository)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _httpContextAccessor = httpContextAccessor;
            _projectRepository = projectRepository;
            _projectMemberRepository = projectMember;
            _boardRepository = boardRepository;
             _mailService = mailService;
            _mapper = mapper;
            _verifyTokenRopository = verifyTokenRopository;
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

        public async Task<ProjectResponse> CreateProject(CreateProjectRequest request)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var UserId = httpContext?.User.FindFirst("id")?.Value;
            //create project
            var projectId = await _projectRepository.CreateProjectAsync(request.title);
            if (projectId == Guid.Empty)
                throw new AppException(ErrorCode.CannotCreateProject);
            //create Pm for the project
            var projectMember = new ProjectMember
            {
                UserId = Guid.Parse(UserId!),
                ProjectId = projectId,
                Role = ProjectRole.PM,
                IsActive = true
            };
            await _projectMemberRepository.CreateProjectMemeberAsync(projectMember);
            //create default boards for the project
            Console.WriteLine(projectId);
            int Order = await _boardRepository.GetMaxOrder(projectId);
            Console.WriteLine(Order);
            var defaultBoards = new List<Board>
            {
                new Board
                {
                    Id = Guid.NewGuid(),
                    ProjectId = projectId,
                    Name = "Not Started",
                    Description = "Tasks that have not been started yet.",
                    Order = ++Order,
                    IsActive = true
                },
                 new Board
                {
                    Id = Guid.NewGuid(),
                    ProjectId = projectId,
                    Name = "In Progress",
                    Description = "Tasks that are currently in progress.",
                    Order = ++Order,
                    IsActive = true
                },
                  new Board
                {
                    Id = Guid.NewGuid(),
                    ProjectId = projectId,
                    Name = "Completed",
                    Description = "Tasks that have been completed.",
                    Order = ++Order,
                    IsActive = true
                },
            };
            await _boardRepository.CreateListBoardsAsync(defaultBoards);
            return new ProjectResponse
            {
                Id = projectId,
                Title = request.title
            };
        }

        public async Task<bool> VerifyJoinProject(string token)
        {
            var verifyToken = await _verifyTokenRopository.GetVerifyTokenAsync(token);
            if (verifyToken == null || verifyToken.IsUsed || verifyToken.IsExpired)
            {
                throw new AppException(ErrorCode.InvalidToken);
            }
            var user = await _projectMemberRepository.FindMemberInProject(verifyToken.ProjectId!.Value, verifyToken.UserId);
            if (user == null)
            {
                throw new AppException(ErrorCode.NoUserFound);
            }
            //Active the user in the project
            user.IsActive = true;
            await _projectMemberRepository.UpdateMember(user);

            //Update token
            verifyToken.IsUsed = true;
            await _verifyTokenRopository.UpdateTokenAsync(verifyToken);
            return true;
        }
    }
}
