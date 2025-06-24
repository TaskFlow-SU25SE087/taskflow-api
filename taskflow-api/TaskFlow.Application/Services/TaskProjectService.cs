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
        private readonly IProjectMemberRepository _projectMemberRepository;

        public TaskProjectService(ITaskProjectRepository taskProjectRepository, IBoardRepository boardRepository,
            IFileService fileService, IMapper mapper, ITaskTagRepository taskTagRepository,
            ITagRepository tagRepository, IHttpContextAccessor httpContextAccessor, 
            ITaskAssigneeRepository taskAssigneeRepository, IProjectMemberRepository projectMemberRepository)
        {
            _taskProjectRepository = taskProjectRepository;
            _boardRepository = boardRepository;
            _fileService = fileService;
            _mapper = mapper;
            _taskTagRepository = taskTagRepository;
            _tagRepository = tagRepository;
            _httpContextAccessor = httpContextAccessor;
            _taskAssigneeRepository = taskAssigneeRepository;
            _projectMemberRepository = projectMemberRepository;
        }

        public async Task AddTagForTask(Guid TaskId, Guid TagId)
        {
            var task = await _taskProjectRepository.GetTaskByIdAsync(TaskId)
              ?? throw new AppException(ErrorCode.TaskNotFound);

            var tag = await _tagRepository.GetTagByIdAsync(TagId)
               ?? throw new AppException(ErrorCode.TagNotFound);

            if (task.ProjectId != tag.ProjectId)
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

        public async Task<TaskProject> AddTask(AddTaskRequest request, Guid ProjectId)
        {
            var BoardId = _boardRepository.GetIdBoardOrderFirtsAsync(ProjectId);
            var task = new TaskProject
            {
                ProjectId = ProjectId,
                Title = request.Title,
                BoardId = BoardId.Result,
                Description = request.Description,
                Priority = request.Priority,
                IsActive = true,
            };
            if (request.File != null)
            {
                var filePath = await _fileService.UploadAutoAsync(request.File);
                task.AttachmentUrl = filePath;
            }
            await _taskProjectRepository.AddTaskAsync(task);
            return task;
        }

        public async Task AssignTaskToUser(Guid TaskId, Guid ProjectId, AssignTaskRequest request)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var UserId = httpContext?.User.FindFirst("id")?.Value;
            //check user is in project
            var UserAssign = await _projectMemberRepository.FindMemberInProject(ProjectId, Guid.Parse(UserId!));
            if (UserAssign == null || request.AssignerId == null)
            {
                throw new AppException(ErrorCode.UserNotInProject);
            }
            //check user already assigned to task
            bool checkExits = await _taskAssigneeRepository.IsTaskAssigneeExistsAsync(TaskId, request.AssignerId);
            if (checkExits)
            {
                throw new AppException(ErrorCode.TaskAlreadyAssigned);
            }

            var newTaskAginee = new TaskAssignee
            {
                ImplementerId = UserAssign.Id,
                AssignerId = request.AssignerId,
                RefId = TaskId,
                Type = RefType.Task,
                IsActive = true,
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
            //var result = _mapper.Map<List<TaskProjectResponse>>(listTask);
            return listTask;

        }

        public async Task LeaveTask(Guid ProjectID, Guid TaskId, AssignmentReasonRequest request)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var UserId = httpContext?.User.FindFirst("id")?.Value;
            var projectmember = await _projectMemberRepository.FindMemberInProject(ProjectID, Guid.Parse(UserId!));
            var taskAssignee = await _taskAssigneeRepository.GetTaskAssigneeByTaskIdAndUserIDAsync(TaskId, projectmember!.Id);
            if (taskAssignee == null)
            {
                throw new AppException(ErrorCode.UserNotAssignedToTask);
            }
            await UpdateTaskAssignmentStatus(taskAssignee.Id, request.Reason, "User voluntarily left the task");

        }

        public async Task RevokeTaskAssignment(Guid ProjectId, Guid TaskId, RemoveAssignmentReasonRequest request)
        {
            var TaskAssignee = await _taskAssigneeRepository.GetTaskAssigneeByTaskIdAndUserIDAsync(TaskId, request.AssigneeId);
            if (TaskAssignee == null)
            {
                throw new AppException(ErrorCode.UserNotAssignedToTask);
            }
            await UpdateTaskAssignmentStatus(TaskAssignee.Id, request.Reason, "Removed from task by project leader");
        }

        public async Task<TaskProject> UpdateTask(UpdateTaskRequest request, Guid TaskId)
        {
            var taskUpdate = await _taskProjectRepository.GetTaskByIdAsync(TaskId);
            if (taskUpdate == null)
            {
                throw new AppException(ErrorCode.TaskNotFound);
            }
            taskUpdate.Title = request.Title;
            taskUpdate.Description = request.Description;
            taskUpdate.Priority = request.Priority;
            taskUpdate.UpdatedAt = DateTime.UtcNow;
            //if (request.File != null)
            //{
            //    var filePath = await _fileService.UploadFileAsync(request.File);
            //    taskUpdate.File = filePath;
            //}
            await _taskProjectRepository.UpdateTaskAsync(taskUpdate);
            return taskUpdate;
        }

        public async Task userAcceptTask(Guid ProjectId, Guid TaskId)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var UserId = httpContext?.User.FindFirst("id")?.Value;
            //get ID member in project
            var member = await _projectMemberRepository.FindMemberInProject(ProjectId, Guid.Parse(UserId!));
            if (member == null)
            {
                throw new AppException(ErrorCode.UserNotInProject);
            }
            //check user accept task
            bool checkExits = await _taskAssigneeRepository.IsTaskAssigneeExistsAsync(TaskId, member.Id);
            if (checkExits)
            {
                throw new AppException(ErrorCode.TaskAlreadyAssigned);
            }

            var newTaskAginee = new TaskAssignee
            {
                ImplementerId = member.Id,
                AssignerId = member.Id,
                RefId = TaskId,
                Type = RefType.Task,
                IsActive = true
            };
            await _taskAssigneeRepository.AcceptTaskAsync(newTaskAginee);
        }

        private async Task UpdateTaskAssignmentStatus(Guid taskAssigneeId, string reason, string defaultNote)
        {
            var taskAssignee = await _taskAssigneeRepository.GetTaskAssigneeAsync(taskAssigneeId);
            if (taskAssignee == null)
            {
                throw new AppException(ErrorCode.UserNotAssignedToTask);
            }
            taskAssignee.UpdatedAt = DateTime.UtcNow;
            taskAssignee.IsActive = false;
            taskAssignee.CancellationNote = string.IsNullOrWhiteSpace(reason)
                                            ? reason
                                            : defaultNote + ": " + reason;
            await _taskAssigneeRepository.UpdateAsync(taskAssignee);
        }
    }
}
