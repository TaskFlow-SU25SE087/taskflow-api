using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using taskflow_api.Migrations;
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
        private readonly AppTimeProvider _timeProvider;

        public ProjectService(UserManager<User> userManager, SignInManager<User> signInManager,
            IHttpContextAccessor httpContextAccessor, IProjectRepository projectRepository,
            IProjectMemberRepository projectMember, IBoardRepository boardRepository,
            IMailService mailService, IMapper mapper, IVerifyTokenRopository verifyTokenRopository,
            ISprintRepository springRepository, ITagRepository TagRepository, ITaskTagRepository taskTagRepository,
            AppTimeProvider timeProvider)
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
            _timeProvider = timeProvider;
        }

        public async Task<ProjectResponse> CreateProject(CreateProjectRequest request)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var UserId = httpContext?.User.FindFirst("id")?.Value;
            var user = await _userManager.Users.
                Include(u => u.Term)
                .FirstOrDefaultAsync(u => u.Id == Guid.Parse(UserId!));

            //Find the number of projects participated
            bool projectCount = await _projectMemberRepository.GetUserIsActiveInProjectAsync(Guid.Parse(UserId!));
            if (projectCount)
            {
                throw new AppException(ErrorCode.MaxProjectLimitReached);
            }

            //create project
            var project = new Project
            {
                Title = request.Title,
                Description = request.Description,
                TermId = user.Term.Id,
                Semester = user.Term.Season + user.Term.Year,
                CreatedAt = _timeProvider.Now,
                IsActive = true
            };
            var projectId = await _projectRepository.CreateProjectAsync(project);
            if (projectId == Guid.Empty)
                throw new AppException(ErrorCode.CannotCreateProject);
            //create Pm for the project
            var projectMember = new ProjectMember
            {
                UserId = Guid.Parse(UserId!),
                ProjectId = projectId,
                Role = ProjectRole.Leader,
                IsActive = true,
                HasJoinedBefore = true,
            };
            await _projectMemberRepository.CreateProjectMemeberAsync(projectMember);
            var UserSystem = new ProjectMember
            {
                UserId = Guid.Parse("00000000-0000-0000-0000-000000000001"), // System user ID
                ProjectId = projectId,
                Role = ProjectRole.System,
                IsActive = true,
                HasJoinedBefore = true,
            };
            //create SPring for the project
            var newSprint = new Sprint
            {
                ProjectId = projectId,
                Name = "Sprint 1",
                Description = "First sprint of the project",
                StartDate = _timeProvider.Now,
                EndDate = _timeProvider.Now.AddDays(14), // Default 2 weeks sprint
                Status = SprintStatus.NotStarted,
                IsActive = true
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
                    Order = Order,
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
                Color = "#FF5733", // Default color
            };
            await _TagRepository.AddTagAsync(Tag);
            var result = _mapper.Map<ProjectResponse>(project);
            return result;
        }

        public async Task<ProjectDetailResponse> GetProject(Guid ProjectId)
        {
            var project = await _projectRepository.GetProjectByIdAsync(ProjectId);

            var result = _mapper.Map<ProjectDetailResponse>(project);
            return result;
        }

        public async Task<List<ProjectsResponse>> ListProjectResponse()
        {
            var userIdStr = _httpContextAccessor.HttpContext?.User.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(userIdStr))
            {
                throw new AppException(ErrorCode.Unauthorized);
            }
            var userId = Guid.Parse(userIdStr);
            var projectsQuery = await _projectRepository.GetListProjectResponseByUserAsync(userId);
            if (projectsQuery == null)
            {
                throw new AppException(ErrorCode.NoProjectsFound);
            }
            return projectsQuery;
        }

        public async Task<ProjectResponse> UpdateProject(UpdateProjectRequest request)
        {
            var project = await _projectRepository.GetProjectByIdAsync(request.ProjectId);
            if (project == null)
            {
                throw new AppException(ErrorCode.ProjectNotFound);
            }
            //Update the project
             project!.Title  = request.Title;
            project!.Description = request.Description;
            project!.LastUpdate = _timeProvider.Now;
            await _projectRepository.UpdateProject(project);
            return _mapper.Map<ProjectResponse>(project);
        }

    }
}
