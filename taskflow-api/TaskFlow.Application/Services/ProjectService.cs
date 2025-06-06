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
        private readonly ISprintRepository _sprintRepository;

        public ProjectService(UserManager<User> userManager, SignInManager<User> signInManager,
            IHttpContextAccessor httpContextAccessor, IProjectRepository projectRepository,
            IProjectMemberRepository projectMember, IBoardRepository boardRepository,
            IMailService mailService, IMapper mapper, IVerifyTokenRopository verifyTokenRopository,
            ISprintRepository springRepository)
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
            _sprintRepository = springRepository;
        }

        public async Task<ProjectResponse> CreateProject(CreateProjectRequest request)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var UserId = httpContext?.User.FindFirst("id")?.Value;
            //create project
            var projectId = await _projectRepository.CreateProjectAsync(request.title, request.description);
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
            //create SPring for the project
            var newSprint = new Sprint
            {
                Id = Guid.NewGuid(),
                ProjectId = projectId,
                Name = "Sprint 1",
                Description = "First sprint of the project",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(14),
                Status = SprintStatus.NotStarted
            };
            await _sprintRepository.CreateSprintAsync(newSprint);

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
                Title = request.title,
                Description = request.description,
            };
        }

        public async Task<ProjectResponse> UpdateProject(UpdateProjectRequest request)
        {
            var project = await _projectRepository.GetProjectByIdAsync(request.ProjectId);
            if (project == null)
            {
                throw new AppException(ErrorCode.ProjectNotFound);
            }
            //Update the project
             project!.Description  = request.Title;
            await _projectRepository.UpdateProject(project);
            return _mapper.Map<ProjectResponse>(project);
        }

    }
}
