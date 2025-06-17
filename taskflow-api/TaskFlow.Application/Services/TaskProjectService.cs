using AutoMapper;
using taskflow_api.TaskFlow.Application.DTOs.Request;
using taskflow_api.TaskFlow.Application.DTOs.Response;
using taskflow_api.TaskFlow.Application.Interfaces;
using taskflow_api.TaskFlow.Domain.Common.Enums;
using taskflow_api.TaskFlow.Domain.Entities;
using taskflow_api.TaskFlow.Infrastructure.Interfaces;
using taskflow_api.TaskFlow.Shared.Exceptions;

namespace taskflow_api.TaskFlow.Application.Services
{
    public class TaskProjectService : ITaskProjectService
    {
        private readonly ITaskProjectRepository _taskProjectRepository;
        private readonly IBoardRepository _boardRepository;
        private readonly IFileService _fileService;
        private readonly IMapper _mapper;
        private readonly ITaskTagRepository _taskTagRepository;
        private readonly ITagRepository _tagRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITaskAssigneeRepository _taskAssigneeRepository;

        public TaskProjectService(ITaskProjectRepository taskProjectRepository, IBoardRepository boardRepository,
            IFileService fileService, IMapper mapper, ITaskTagRepository taskTagRepository,
            ITagRepository tagRepository, IHttpContextAccessor httpContextAccessor, 
            ITaskAssigneeRepository taskAssigneeRepository)
        {
            _taskProjectRepository = taskProjectRepository;
            _boardRepository = boardRepository;
            _fileService = fileService;
            _mapper = mapper;
            _taskTagRepository = taskTagRepository;
            _tagRepository = tagRepository;
            _httpContextAccessor = httpContextAccessor;
            _taskAssigneeRepository = taskAssigneeRepository;
        }

        public async Task AddTagForTask(Guid ProjectId, Guid TaskId, Guid TagId)
        {
            var task = await _taskProjectRepository.GetTaskByIdAsync(TaskId)
              ?? throw new AppException(ErrorCode.TaskNotFound);

            var tag = await _tagRepository.GetTagByIdAsync(TagId)
               ?? throw new AppException(ErrorCode.TagNotFound);

            if (!task.ProjectId.Equals(tag.ProjectId))
            {
                throw new AppException(ErrorCode.NoPermission);
            }
            var existingTaskTag = await _taskTagRepository.GetTaskTagAsync(TaskId, TagId);
            if (existingTaskTag != null)
            {
                throw new AppException(ErrorCode.TagAlreadyExistsInTask);
            }

            var newTaskTag = new TaskTag
            {
                TagId = TagId,
                TaskId = TaskId
            };
            await _taskTagRepository.AddTaskTagAsync(newTaskTag);
        }

        public async Task<TaskProject> AddTask(AddTaskRequest request)
        {
            var BoardId = _boardRepository.GetIdBoardOrderFirtsAsync(request.ProjectId);
            var task = new TaskProject
            {
                ProjectId = request.ProjectId,
                Title = request.Title,
                BoardId = BoardId.Result,
                Description = request.Description,
                Priority = request.Priority,
                IsActive = true,
            };
            await _taskProjectRepository.AddTaskAsync(task);
            return task;
        }

        public async Task AssignTaskToUser(Guid TaskId, Guid AssignerId)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var UserId = httpContext?.User.FindFirst("id")?.Value;
            bool checkExits = await _taskAssigneeRepository.IsTaskAssigneeExistsAsync(TaskId, AssignerId);
            if (checkExits)
            {
                throw new AppException(ErrorCode.TaskAlreadyAssigned);
            }
            var newTaskAginee = new TaskAssignee
            {
                ImplementerId = Guid.Parse(UserId!),
                AssignerId = AssignerId,
                RefId = TaskId,
                Type = RefType.Task
            };
            await _taskAssigneeRepository.AcceptTaskAsync(newTaskAginee);
        }

        public async Task<bool> DeleteTask(Guid taskId)
        {
            var deleteTask = await _taskProjectRepository.GetTaskByIdAsync(taskId);
            if (deleteTask == null)
            {
                throw new AppException(ErrorCode.TaskNotFound);
            }
            //check :.....
            deleteTask.IsActive = false;
            await _taskProjectRepository.UpdateTaskAsync(deleteTask);
            return true;
        }

        public async Task<List<TaskProjectResponse>> GetAllTask(Guid projectId)
        {
            var listTask = await _taskProjectRepository.GetAllTaskProjectAsync(projectId);
            var result = _mapper.Map<List<TaskProjectResponse>>(listTask);
            return result;

        }

        public async Task<TaskProject> UpdateTask(UpdateTaskRequest request)
        {
            var taskUpdate = await _taskProjectRepository.GetTaskByIdAsync(request.Id);
            if (taskUpdate == null)
            {
                throw new AppException(ErrorCode.TaskNotFound);
            }
            taskUpdate.Title = request.Title;
            taskUpdate.Description = request.Description;
            taskUpdate.Priority = request.Priority;
            taskUpdate.BoardId = request.BoardID;
            taskUpdate.SprintId = request.SprintId;
            taskUpdate.UpdatedAt = DateTime.UtcNow;
            if (request.File != null)
            {
                var filePath = await _fileService.UploadFileAsync(request.File);
                taskUpdate.File = filePath;
            }
            await _taskProjectRepository.UpdateTaskAsync(taskUpdate);
            return taskUpdate;
        }

        public async Task userAcceptTask(Guid TaskId)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var UserId = httpContext?.User.FindFirst("id")?.Value;

            bool checkExits = await _taskAssigneeRepository.IsTaskAssigneeExistsAsync(TaskId, Guid.Parse(UserId!));
            if (checkExits)
            {
                throw new AppException(ErrorCode.TaskAlreadyAssigned);
            }
            var newTaskAginee = new TaskAssignee
            {
                ImplementerId = Guid.Parse(UserId!),
                AssignerId = Guid.Parse(UserId!),
                RefId = TaskId,
                Type = RefType.Task
            };
            await _taskAssigneeRepository.AcceptTaskAsync(newTaskAginee);
        }
    }
}
