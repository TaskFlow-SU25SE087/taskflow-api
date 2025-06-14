using AutoMapper;
using Microsoft.AspNetCore.Identity;
using taskflow_api.TaskFlow.Application.DTOs.Common;
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
        private readonly ITagRepository _TagRepository;
        private readonly ITaskTagRepository _taskTagRepository;

        public ProjectService(UserManager<User> userManager, SignInManager<User> signInManager,
            IHttpContextAccessor httpContextAccessor, IProjectRepository projectRepository,
            IProjectMemberRepository projectMember, IBoardRepository boardRepository,
            IMailService mailService, IMapper mapper, IVerifyTokenRopository verifyTokenRopository,
            ISprintRepository springRepository, ITagRepository TagRepository, ITaskTagRepository taskTagRepository)
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
            _TagRepository = TagRepository;
            _taskTagRepository = taskTagRepository;
        }

        public async Task<ProjectResponse> CreateProject(CreateProjectRequest request)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var UserId = httpContext?.User.FindFirst("id")?.Value;
            var user = await _userManager.FindByIdAsync(UserId!);
            //create project
            var projectId = await _projectRepository.CreateProjectAsync(request.title, request.description, Guid.Parse(UserId!));
            if (projectId == Guid.Empty)
                throw new AppException(ErrorCode.CannotCreateProject);
            //create Pm for the project
            var projectMember = new ProjectMember
            {
                UserId = Guid.Parse(UserId!),
                ProjectId = projectId,
                Role = ProjectRole.Leader,
                IsActive = true
            };
            await _projectMemberRepository.CreateProjectMemeberAsync(projectMember);
            //create SPring for the project
            var newSprint = new Sprint
            {
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

            //create Tags for the project
            var Tag = new Tag
            {
                Id = Guid.NewGuid(),
                Name = user!.FullName,
                ProjectId = projectId,
                Description = "Tag of "+ user!.FullName,
            };
            await _TagRepository.AddTagAsync(Tag);
            return new ProjectResponse
            {
                Id = projectId,
                Title = request.title,
                Description = request.description,
            };
        }

        public async Task<ProjectDetailResponse> GetProject(Guid ProjectId)
        {
            var project = await _projectRepository.GetProjectByIdAsync(ProjectId);

            var result = _mapper.Map<ProjectDetailResponse>(project);
            return result;
        }

        public async Task<PagedResult<ProjectsResponse>> ListProjectResponse(int Page)
        {
            var userIdStr = _httpContextAccessor.HttpContext?.User.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(userIdStr))
            {
                throw new AppException(ErrorCode.Unauthorized);
            }
            var userId = Guid.Parse(userIdStr);
            //get Projects by userId
            var projectsQuery = _projectRepository.GetProjectsByUserIdAsync(userId);
            if (projectsQuery == null)
            {
                throw new AppException(ErrorCode.NoProjectsFound);
            }
            //Page the projects
            PagingParams pagingParams = new PagingParams
            {
                PageNumber = Page,
                PageSize = 10
            };
            var pagedProjects = await projectsQuery.ToPagedListAsync(pagingParams);
            // map the projects to response DTO
            var responseList = new List<ProjectsResponse>();
            foreach (var project in pagedProjects.Items)
            {
                var response = _mapper.Map<ProjectsResponse>(project, opt =>
                {
                    opt.Items["userId"] = userId;
                });
                response.Role = project.Members.FirstOrDefault(m => m.UserId == userId)?.Role;
                responseList.Add(response);
            }
            return new PagedResult<ProjectsResponse>
            {
                Items = responseList,
                TotalItems = pagedProjects.TotalItems,
                PageNumber = pagedProjects.PageNumber,
                PageSize = pagedProjects.PageSize,
                TotalPages = pagedProjects.TotalPages
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
