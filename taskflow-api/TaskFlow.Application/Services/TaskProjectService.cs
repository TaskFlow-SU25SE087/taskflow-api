﻿using AutoMapper;
using System;
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
        private readonly ISprintRepository _sprintRepository;
        private readonly INotificationService _notificationService;
        private readonly AppTimeProvider _timeProvider;

        public TaskProjectService(ITaskProjectRepository taskProjectRepository, IBoardRepository boardRepository,
            IFileService fileService, IMapper mapper, ITaskTagRepository taskTagRepository,
            ITagRepository tagRepository, IHttpContextAccessor httpContextAccessor, 
            ITaskAssigneeRepository taskAssigneeRepository, IProjectMemberRepository projectMemberRepository,
            ISprintRepository sprintRepository, AppTimeProvider timeProvider,
            INotificationService notificationService)
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
            _sprintRepository = sprintRepository;
            _notificationService = notificationService;
            _timeProvider = timeProvider;
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

        public async Task AddTask(AddTaskRequest request, Guid ProjectId)
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
                CreatedAt = _timeProvider.Now,
            };
            if (request.File != null)
            {
                var filePath = await _fileService.UploadAutoAsync(request.File);
                task.AttachmentUrl = filePath;
            }
            await _taskProjectRepository.AddTaskAsync(task);
        }

        public async Task AssignTaskToUser(Guid TaskId, Guid ProjectId, AssignTaskRequest request)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var UserId = httpContext?.User.FindFirst("id")?.Value;
            //check user is in project
            var UserAssign = await _projectMemberRepository.FindMemberInProject(ProjectId, Guid.Parse(UserId!));
            if (UserAssign == null || request.ImplementerId == null)
            {
                throw new AppException(ErrorCode.UserNotInProject);
            }
            //check user already assigned to task
            bool checkExits = await _taskAssigneeRepository.IsTaskAssigneeExistsAsync(TaskId, request.ImplementerId);
            if (checkExits)
            {
                throw new AppException(ErrorCode.TaskAlreadyAssigned);
            }

            var newTaskAginee = new TaskAssignee
            {
                AssignerId = UserAssign.Id,
                ImplementerId = request.ImplementerId,
                RefId = TaskId,
                Type = RefType.Task,
                IsActive = true,
                CreatedAt = _timeProvider.Now
            };
            await _taskAssigneeRepository.AcceptTaskAsync(newTaskAginee);
        }

        public async Task ChangeBoard(Guid BoardId, Guid TaskId)
        {
            var taskProject = await _taskProjectRepository.GetTaskByIdAsync(TaskId);
            if (taskProject!.BoardId == BoardId)
            {
                throw new AppException(ErrorCode.TaskAlreadyInThisBoard);
            }

            // Get old board info
            var oldBoard = await _boardRepository.GetBoardByIdAsync(taskProject.BoardId.HasValue ? taskProject.BoardId.Value : Guid.Empty);
            var oldBoardName = oldBoard?.Name ?? "Unknown";

            var sprint = await  _sprintRepository.GetSprintByIdAsync(taskProject.SprintId.HasValue ? taskProject.SprintId.Value : Guid.Empty);
            if (!sprint!.Status.Equals(SprintStatus.InProgress))
            {
                throw new AppException(ErrorCode.CannotUpdateStatus);
            }

            // Change board
            taskProject!.BoardId = BoardId;
            await _taskProjectRepository.UpdateTaskAsync(taskProject);

            // Get new board info
            var newBoard = await _boardRepository.GetBoardByIdAsync(BoardId);
            var newBoardName = newBoard?.Name ?? "Unknown";

            // Get all assignees for the task
            var assignees = await _taskAssigneeRepository.taskAssigneesAsync(TaskId);
            var projectMemberIds = assignees
                .Where(a => a.ImplementerId.HasValue)
                .Select(a => a.ImplementerId.Value)
                .ToList();

            await _notificationService.NotifyTaskBoardChangeAsync(taskProject.ProjectId, TaskId, oldBoardName, newBoardName, projectMemberIds);
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

        public Task<List<ListTaskProjectNotSprint>> GettAllTaskNotSprint(Guid ProjectId)
        {
            return _taskProjectRepository.GetAllTaskNotSprint(ProjectId);
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
            var TaskAssignee = await _taskAssigneeRepository.GetTaskAssigneeByTaskIdAndUserIDAsync(TaskId, request.ImplementId);
            if (TaskAssignee == null)
            {
                throw new AppException(ErrorCode.UserNotAssignedToTask);
            }
            await UpdateTaskAssignmentStatus(TaskAssignee.Id, request.Reason, "Removed from task by project leader");
        }

        public async Task SubmitTaskCompletion(Guid Project, Guid taskId, CompleteTaskRequest request)
        {
            var userID = _httpContextAccessor.HttpContext?.User.FindFirst("id")?.Value;
            var memberproject = await _projectMemberRepository.FindMemberInProject(Project, Guid.Parse(userID!));
            if (memberproject == null)
            {
                throw new AppException(ErrorCode.UserNotInProject);
            }
            //check task exists
            var task = await _taskProjectRepository.GetTaskByIdAsync(taskId);
            if (task == null)
            {
                throw new AppException(ErrorCode.TaskNotFound);
            }
            //check user is in task
            var taskAssignee = await _taskAssigneeRepository.GetTaskAssigneeByTaskIdAndUserIDAsync(taskId, memberproject!.Id);
            if (taskAssignee == null)
            {
                throw new AppException(ErrorCode.UserNotAssignedToTask);
            }

            //have files
            if (request.Files != null && request.Files.Any())
            {
                var urls = new List<string>();
                foreach (var file in request.Files)
                {
                    var fileUrl = await _fileService.UploadAutoAsync(file);
                    urls.Add(fileUrl);
                }
                // save file urls to the task
                task!.CompletionAttachmentUrlsList = urls;
                await _taskProjectRepository.UpdateTaskAsync(task);
            }
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
            taskUpdate.UpdatedAt = _timeProvider.Now;

            await _taskProjectRepository.UpdateTaskAsync(taskUpdate);

            // Notify all assignees about the update
            var assignees = await _taskAssigneeRepository.taskAssigneesAsync(TaskId);
            foreach (var assignee in assignees)
            {
                if (assignee.ImplementerId.HasValue)
                {
                    // Get the ProjectMember to find the UserId
                    var projectMember = await _projectMemberRepository.FindMemberInProjectByProjectMemberID(assignee.ImplementerId.Value);
                    if (projectMember != null)
                    {
                        await _notificationService.NotifyTaskUpdateAsync(projectMember.UserId, taskUpdate.ProjectId, TaskId, $"Task '{taskUpdate.Title}' has been updated.");
                    }
                }
            }

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
                CreatedAt = _timeProvider.Now,
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
            taskAssignee.UpdatedAt = _timeProvider.Now;
            taskAssignee.IsActive = false;
            taskAssignee.CancellationNote = string.IsNullOrWhiteSpace(reason)
                                            ? reason
                                            : defaultNote + ": " + reason;
            await _taskAssigneeRepository.UpdateAsync(taskAssignee);
        }
    }
}
