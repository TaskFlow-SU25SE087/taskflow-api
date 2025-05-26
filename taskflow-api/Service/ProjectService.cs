using Microsoft.AspNetCore.Identity;
using taskflow_api.Entity;
using taskflow_api.Enums;
using taskflow_api.Exceptions;
using taskflow_api.Model.Request;
using taskflow_api.Model.Response;
using taskflow_api.Repository;

namespace taskflow_api.Service
{
    public class ProjectService : IProjectService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IProjectRepository _projectRepository;
        private readonly IProjectMemberRepository _projectMemberRepository;
        private readonly IBoardRepository _boardRepository;

        public ProjectService(UserManager<User> userManager, SignInManager<User> signInManager,
            IHttpContextAccessor httpContextAccessor, IProjectRepository projectRepository,
            IProjectMemberRepository projectMember, IBoardRepository boardRepository)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _httpContextAccessor = httpContextAccessor;
            _projectRepository = projectRepository;
            _projectMemberRepository = projectMember;
            _boardRepository = boardRepository;
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
            var defaultBoards = new List<Board>
            {
                new Board
                {
                    Id = Guid.NewGuid(),
                    ProjectId = projectId,
                    Name = "Not Started",
                    Description = "Tasks that have not been started yet.",
                    IsActive = true
                },
                 new Board
                {
                    Id = Guid.NewGuid(),
                    ProjectId = projectId,
                    Name = "In Progress",
                    Description = "Tasks that are currently in progress.",
                    IsActive = true
                },
                  new Board
                {
                    Id = Guid.NewGuid(),
                    ProjectId = projectId,
                    Name = "Completed",
                    Description = "Tasks that have been completed.",
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
    }
}
