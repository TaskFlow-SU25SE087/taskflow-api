using AutoMapper;
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
        private readonly INotificationService _notificationService;
        private readonly IEffortPointsService _effortPointsService;
        private readonly AppTimeProvider _timeProvider;
        private readonly ILogProjectService _logService;
        private readonly ISprintRepository _sprintRepository;

        public TaskProjectService(ITaskProjectRepository taskProjectRepository, IBoardRepository boardRepository,
            IFileService fileService, IMapper mapper, ITaskTagRepository taskTagRepository,
            ITagRepository tagRepository, IHttpContextAccessor httpContextAccessor, 
            ITaskAssigneeRepository taskAssigneeRepository, IProjectMemberRepository projectMemberRepository,
            IEffortPointsService effortPointsService, AppTimeProvider timeProvider,
            INotificationService notificationService, ILogProjectService logService, ISprintRepository sprintRepository)
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
            _effortPointsService = effortPointsService;
            _notificationService = notificationService;
            _timeProvider = timeProvider;
            _logService = logService;
            _sprintRepository = sprintRepository;
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

        public async Task RemoveTagFromTask(Guid TaskId, Guid TagId)
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
            if (existingTaskTag == null)
            {
                throw new AppException(ErrorCode.TagNotFound);
            }

            await _taskTagRepository.RemoveTaskTagAsync(TaskId, TagId);
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
                Deadline = request.Deadline,
                EffortPoints = request.EffortPoints,
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
            if (UserAssign == null)
            {
                throw new AppException(ErrorCode.UserNotInProject);
            }
            //check user already assigned to task
            bool checkExits = await _taskAssigneeRepository.IsTaskAssigneeExistsAsync(TaskId, request.ImplementerId);
            if (checkExits)
            {
                throw new AppException(ErrorCode.TaskAlreadyAssigned);
            }

            // Get task information for notification
            var task = await _taskProjectRepository.GetTaskByIdAsync(TaskId);
            if (task == null)
            {
                throw new AppException(ErrorCode.TaskNotFound);
            }

            // Get current assignees to determine if this will be multiple assignees
            var currentAssignees = await _taskAssigneeRepository.taskAssigneesAsync(TaskId);
            var willHaveMultipleAssignees = currentAssignees.Count > 0;

            // Handle effort points based on assignment scenario
            int? assignedEffortPoints = null;
            if (willHaveMultipleAssignees)
            {
                // Multiple assignees - use automatic distribution
                if (task.EffortPoints.HasValue)
                {
                    // Task has effort points - distribute equally among all assignees
                    var allAssignees = new List<TaskAssignee>(currentAssignees);
                    var newAssignee = new TaskAssignee
                    {
                        AssignerId = UserAssign.Id,
                        ImplementerId = request.ImplementerId,
                        RefId = TaskId,
                        Type = RefType.Task,
                        IsActive = true,
                        CreatedAt = _timeProvider.Now
                    };
                    allAssignees.Add(newAssignee);
                    
                    var distribution = _effortPointsService.DistributeEffortPointsEqually(task.EffortPoints.Value, allAssignees.Count);
                    
                    // Update existing assignees with new distribution
                    for (int i = 0; i < currentAssignees.Count; i++)
                    {
                        currentAssignees[i].AssignedEffortPoints = distribution[i];
                    }
                    await _taskAssigneeRepository.UpdateMultipleTaskAssigneesAsync(currentAssignees);
                    
                    // Set effort points for new assignee
                    assignedEffortPoints = distribution[currentAssignees.Count];
                    newAssignee.AssignedEffortPoints = assignedEffortPoints;
                    
                    await _taskAssigneeRepository.AcceptTaskAsync(newAssignee);
                }
                else
                {
                    // Task doesn't have effort points - set effort points from assignee request
                    assignedEffortPoints = request.AssignedEffortPoints;
                    var newAssignee = new TaskAssignee
                    {
                        AssignerId = UserAssign.Id,
                        ImplementerId = request.ImplementerId,
                        RefId = TaskId,
                        Type = RefType.Task,
                        AssignedEffortPoints = assignedEffortPoints,
                        IsActive = true,
                        CreatedAt = _timeProvider.Now
                    };
                    await _taskAssigneeRepository.AcceptTaskAsync(newAssignee);
                    
                    // Update task effort points to total of all assignees
                    var totalEffortPoints = await _effortPointsService.CalculateTaskEffortPointsFromAssignees(TaskId);
                    task.EffortPoints = totalEffortPoints;
                    await _taskProjectRepository.UpdateTaskAsync(task);
                }
            }
            else
            {
                // Single assignee - synchronize task and assignee effort points
                if (task.EffortPoints.HasValue)
                {
                    // Task has effort points - assign all to the single assignee
                    assignedEffortPoints = task.EffortPoints.Value;
                }
                else
                {
                    // Task doesn't have effort points - use requested effort points and set task effort points
                    assignedEffortPoints = request.AssignedEffortPoints;
                    if (assignedEffortPoints.HasValue)
                    {
                        task.EffortPoints = assignedEffortPoints.Value;
                        await _taskProjectRepository.UpdateTaskAsync(task);
                    }
                }
                
                var newAssignee = new TaskAssignee
                {
                    AssignerId = UserAssign.Id,
                    ImplementerId = request.ImplementerId,
                    RefId = TaskId,
                    Type = RefType.Task,
                    AssignedEffortPoints = assignedEffortPoints,
                    IsActive = true,
                    CreatedAt = _timeProvider.Now
                };
                await _taskAssigneeRepository.AcceptTaskAsync(newAssignee);
            }

            // Get assigner information for notification
            var assignerName = UserAssign.User?.FullName ?? UserAssign.User?.UserName ?? "Project Member";

            // Send notification to the assigned user
            await _notificationService.NotifyTaskAssignmentAsync(
                request.ImplementerId,
                ProjectId,
                TaskId,
                task.Title,
                assignerName
            );
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

            // Note: Sprint status validation removed as sprint repository is no longer available in this service
            // Sprint validation should be handled at the controller level if needed

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

        public async Task<List<TaskProjectResponse>> GetTasksByBoardType(Guid projectId, BoardType boardType)
        {
            var board = await _boardRepository.GetBoardByTypeAsync(projectId, boardType);
            if (board == null)
            {
                return new List<TaskProjectResponse>();
            }

            var tasks = await _taskProjectRepository.GetTasksByBoardIdAsync(board.Id);
            return tasks;
        }

        public async Task<bool> IsTaskCompleted(Guid taskId)
        {
            var task = await _taskProjectRepository.GetTaskByIdAsync(taskId);
            if (task == null)
            {
                return false;
            }

            if (!task.BoardId.HasValue)
            {
                return false;
            }

            var board = await _boardRepository.GetBoardByIdAsync(task.BoardId.Value);
            return board?.Type == BoardType.Done;
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
                
                // Move task to Done board automatically when completion is submitted
                var doneBoard = await _boardRepository.GetBoardByTypeAsync(Project, BoardType.Done);
                if (doneBoard != null)
                {
                    task.BoardId = doneBoard.Id;
                }
                
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
            
            // Check if effort points are being updated
            var effortPointsChanged = taskUpdate.EffortPoints != request.EffortPoints;
            var oldEffortPoints = taskUpdate.EffortPoints;
            
            taskUpdate.Title = request.Title;
            taskUpdate.Description = request.Description;
            taskUpdate.Priority = request.Priority;
            taskUpdate.Deadline = request.Deadline;
            taskUpdate.EffortPoints = request.EffortPoints;
            taskUpdate.UpdatedAt = _timeProvider.Now;

            await _taskProjectRepository.UpdateTaskAsync(taskUpdate);
            
            // If effort points changed, redistribute among assignees
            if (effortPointsChanged && request.EffortPoints.HasValue)
            {
                var redistributedAssignees = await _effortPointsService.RedistributeEffortPointsOnTaskUpdate(TaskId, request.EffortPoints.Value);
                if (redistributedAssignees.Count > 0)
                {
                    await _taskAssigneeRepository.UpdateMultipleTaskAssigneesAsync(redistributedAssignees);
                }
            }

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

            // Get task information for notification
            var task = await _taskProjectRepository.GetTaskByIdAsync(TaskId);
            if (task == null)
            {
                throw new AppException(ErrorCode.TaskNotFound);
            }

            // Get user information for notification
            var userName = member.User?.FullName ?? member.User?.UserName ?? "User";

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

            // Send notification to the user who accepted the task
            await _notificationService.NotifyTaskAssignmentAsync(
                member.Id,
                ProjectId,
                TaskId,
                task.Title,
                userName
            );
        }

        private async Task UpdateTaskAssignmentStatus(Guid taskAssigneeId, string reason, string defaultNote)
        {
            var taskAssignee = await _taskAssigneeRepository.GetTaskAssigneeAsync(taskAssigneeId);
            if (taskAssignee == null)
            {
                throw new AppException(ErrorCode.UserNotAssignedToTask);
            }
            
            // Get task information for effort point redistribution
            var task = await _taskProjectRepository.GetTaskByIdAsync(taskAssignee.RefId);
            if (task != null && task.EffortPoints.HasValue)
            {
                // Get remaining assignees after this one leaves (excluding the one leaving)
                var allAssignees = await _taskAssigneeRepository.taskAssigneesAsync(taskAssignee.RefId);
                var remainingAssignees = allAssignees.Where(a => a.Id != taskAssigneeId).ToList();
                
                if (remainingAssignees.Count > 0)
                {
                    if (remainingAssignees.Count == 1)
                    {
                        // Only one assignee remaining - give them all effort points
                        remainingAssignees[0].AssignedEffortPoints = task.EffortPoints.Value;
                    }
                    else
                    {
                        // Multiple assignees remaining - redistribute equally
                        var distribution = _effortPointsService.DistributeEffortPointsEqually(task.EffortPoints.Value, remainingAssignees.Count);
                        
                        for (int i = 0; i < remainingAssignees.Count; i++)
                        {
                            remainingAssignees[i].AssignedEffortPoints = distribution[i];
                        }
                    }
                    
                    await _taskAssigneeRepository.UpdateMultipleTaskAssigneesAsync(remainingAssignees);
                }
                // If no assignees remain, task keeps its original effort points (no change needed)
            }
            
            taskAssignee.UpdatedAt = _timeProvider.Now;
            taskAssignee.IsActive = false;
            taskAssignee.CancellationNote = string.IsNullOrWhiteSpace(reason)
                                            ? reason
                                            : defaultNote + ": " + reason;
            await _taskAssigneeRepository.UpdateAsync(taskAssignee);
        }



        public async Task<TaskCompletionSummaryResponse> GetTaskCompletionReport(Guid projectId, TaskCompletionReportRequest request)
        {
            // Get all tasks for the project with related data
            var tasks = await _taskProjectRepository.GetTasksWithDetailsAsync(projectId);

            // Apply filters
            var filteredTasks = tasks.Where(t => 
                (!request.SprintId.HasValue || t.SprintId == request.SprintId) &&
                (!request.Status.HasValue || t.Board?.Type == request.Status) &&
                (!request.Priority.HasValue || t.Priority == request.Priority) &&
                (!request.StartDate.HasValue || t.CreatedAt >= request.StartDate) &&
                (!request.EndDate.HasValue || t.CreatedAt <= request.EndDate) &&
                (!request.IsOverdue.HasValue || (request.IsOverdue.Value ? (t.Deadline.HasValue && DateTime.UtcNow > t.Deadline.Value) : (t.Deadline.HasValue && DateTime.UtcNow <= t.Deadline.Value))) &&
                (request.IncludeCompleted || t.Board?.Type != BoardType.Done) &&
                (request.IncludeInProgress || t.Board?.Type != BoardType.InProgress) &&
                (request.IncludeTodo || t.Board?.Type != BoardType.Todo)
            ).ToList();

            // Filter by assignee if specified
            if (request.AssigneeId.HasValue)
            {
                filteredTasks = filteredTasks.Where(t => 
                    t.TaskAssignees.Any(ta => ta.ImplementerId == request.AssigneeId.Value)
                ).ToList();
            }

            var reportTasks = new List<TaskCompletionReportResponse>();
            var now = DateTime.UtcNow;

            foreach (var task in filteredTasks)
            {
                var isOverdue = task.Deadline.HasValue && now > task.Deadline.Value && task.Board?.Type != BoardType.Done;
                var completedAt = task.Board?.Type == BoardType.Done ? task.UpdatedAt : (DateTime?)null;
                
                // Calculate time spent (simplified - using time from creation to completion or current time)
                TimeSpan? timeSpent = null;
                if (task.Board?.Type == BoardType.Done)
                {
                    timeSpent = task.UpdatedAt - task.CreatedAt;
                }
                else if (task.Board?.Type == BoardType.InProgress)
                {
                    timeSpent = now - task.CreatedAt;
                }

                var assignees = new List<TaskAssigneeReportResponse>();
                foreach (var assignee in task.TaskAssignees)
                {
                    if (assignee.ProjectMember != null)
                    {
                        assignees.Add(new TaskAssigneeReportResponse
                        {
                            ProjectMemberId = assignee.ProjectMember.Id,
                            AssigneeName = assignee.ProjectMember.User?.FullName ?? "Unknown",
                            AssigneeAvatar = assignee.ProjectMember.User?.Avatar,
                            Role = assignee.ProjectMember.Role,
                            AssignedAt = assignee.CreatedAt,
                            CompletedAt = completedAt,
                            TimeSpent = timeSpent
                        });
                    }
                }

                var tags = task.TaskTags.Select(tt => tt.Tag?.Name ?? "").Where(t => !string.IsNullOrEmpty(t)).ToList();

                reportTasks.Add(new TaskCompletionReportResponse
                {
                    TaskId = task.Id,
                    TaskTitle = task.Title,
                    TaskDescription = task.Description,
                    Priority = task.Priority,
                    CreatedAt = task.CreatedAt,
                    Deadline = task.Deadline,
                    CompletedAt = completedAt,
                    IsOverdue = isOverdue,
                    TimeSpent = timeSpent,
                    Status = task.Board?.Type.ToString() ?? "Unknown",
                    Assignees = assignees,
                    SprintName = task.Sprint?.Name,
                    BoardName = task.Board?.Name,
                    Tags = tags
                });
            }

            // Calculate summary statistics
            var totalTasks = reportTasks.Count;
            var todoTasks = reportTasks.Count(t => t.Status == BoardType.Todo.ToString());
            var inProgressTasks = reportTasks.Count(t => t.Status == BoardType.InProgress.ToString());
            var completedTasks = reportTasks.Count(t => t.Status == BoardType.Done.ToString());
            var overdueTasks = reportTasks.Count(t => t.IsOverdue);
            var completionRate = totalTasks > 0 ? (double)completedTasks / totalTasks * 100 : 0;
            
            var totalTimeSpent = TimeSpan.Zero;
            foreach (var task in reportTasks.Where(t => t.TimeSpent.HasValue))
            {
                totalTimeSpent += task.TimeSpent.Value;
            }

            return new TaskCompletionSummaryResponse
            {
                TotalTasks = totalTasks,
                TodoTasks = todoTasks,
                InProgressTasks = inProgressTasks,
                CompletedTasks = completedTasks,
                OverdueTasks = overdueTasks,
                CompletionRate = completionRate,
                TotalTimeSpent = totalTimeSpent,
                Tasks = reportTasks
            };
        }

        public async Task BulkAssignTaskToUsers(Guid TaskId, Guid ProjectId, BulkAssignTaskRequest request)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var UserId = httpContext?.User.FindFirst("id")?.Value;
            
            // Check user is in project
            var UserAssign = await _projectMemberRepository.FindMemberInProject(ProjectId, Guid.Parse(UserId!));
            if (UserAssign == null)
            {
                throw new AppException(ErrorCode.UserNotInProject);
            }

            // Get task information
            var task = await _taskProjectRepository.GetTaskByIdAsync(TaskId);
            if (task == null)
            {
                throw new AppException(ErrorCode.TaskNotFound);
            }

            // Handle effort points based on number of assignees
            var totalAssignedPoints = request.Assignees.Sum(a => a.AssignedEffortPoints ?? 0);
            
            if (request.Assignees.Count == 1)
            {
                // Single assignee - synchronize task and assignee effort points
                if (task.EffortPoints.HasValue)
                {
                    // Task has effort points - assign all to the single assignee
                    var singleAssignee = request.Assignees.First();
                    singleAssignee.AssignedEffortPoints = task.EffortPoints.Value;
                    totalAssignedPoints = task.EffortPoints.Value;
                }
                else
                {
                    // Task doesn't have effort points - set task effort points to assignee's effort points
                    if (totalAssignedPoints > 0)
                    {
                        task.EffortPoints = totalAssignedPoints;
                        await _taskProjectRepository.UpdateTaskAsync(task);
                    }
                }
            }
            else
            {
                // Multiple assignees - validate effort points distribution if task has effort points
                if (task.EffortPoints.HasValue)
                {
                    if (totalAssignedPoints != task.EffortPoints.Value)
                    {
                        throw new AppException(ErrorCode.InvalidEffortPointsDistribution);
                    }
                }
            }

            // Check for duplicate assignees
            var assigneeIds = request.Assignees.Select(a => a.ImplementerId).ToList();
            if (assigneeIds.Count != assigneeIds.Distinct().Count())
            {
                throw new AppException(ErrorCode.DuplicateAssignee);
            }

            // Check if any assignee is already assigned to this task
            foreach (var assignee in request.Assignees)
            {
                bool checkExists = await _taskAssigneeRepository.IsTaskAssigneeExistsAsync(TaskId, assignee.ImplementerId);
                if (checkExists)
                {
                    throw new AppException(ErrorCode.TaskAlreadyAssigned);
                }
            }

            // Create task assignees
            var taskAssignees = new List<TaskAssignee>();
            foreach (var assignee in request.Assignees)
            {
                var newTaskAssignee = new TaskAssignee
                {
                    AssignerId = UserAssign.Id,
                    ImplementerId = assignee.ImplementerId,
                    RefId = TaskId,
                    Type = RefType.Task,
                    AssignedEffortPoints = assignee.AssignedEffortPoints,
                    IsActive = true,
                    CreatedAt = _timeProvider.Now
                };
                taskAssignees.Add(newTaskAssignee);
            }

            await _taskAssigneeRepository.CreateListTaskAssignee(taskAssignees);

            // Send notifications to all assigned users
            var assignerName = UserAssign.User?.FullName ?? UserAssign.User?.UserName ?? "Project Member";
            foreach (var assignee in request.Assignees)
            {
                await _notificationService.NotifyTaskAssignmentAsync(
                    assignee.ImplementerId,
                    ProjectId,
                    TaskId,
                    task.Title,
                    assignerName
                );
            }
        }

        public async Task UpdateIndividualAssigneeEffortPoints(Guid TaskId, Guid ProjectId, UpdateAssigneeEffortPointsRequest request)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var UserId = httpContext?.User.FindFirst("id")?.Value;
            
            // Check user is in project
            var UserAssign = await _projectMemberRepository.FindMemberInProject(ProjectId, Guid.Parse(UserId!));
            if (UserAssign == null)
            {
                throw new AppException(ErrorCode.UserNotInProject);
            }

            // Get task information
            var task = await _taskProjectRepository.GetTaskByIdAsync(TaskId);
            if (task == null)
            {
                throw new AppException(ErrorCode.TaskNotFound);
            }

            // Validate task has effort points
            if (!task.EffortPoints.HasValue)
            {
                throw new AppException(ErrorCode.InvalidEffortPointsDistribution);
            }

            // Update individual assignee effort points and redistribute among others
            var updatedAssignees = await _effortPointsService.UpdateIndividualAssigneeEffortPoints(
                TaskId, 
                task.EffortPoints.Value, 
                request.ProjectMemberId, 
                request.AssignedEffortPoints
            );

            if (updatedAssignees.Count > 0)
            {
                await _taskAssigneeRepository.UpdateMultipleTaskAssigneesAsync(updatedAssignees);
            }

            // Send notification to the updated assignee
            var assignerName = UserAssign.User?.FullName ?? UserAssign.User?.UserName ?? "Project Member";
            await _notificationService.NotifyTaskUpdateAsync(
                request.ProjectMemberId,
                ProjectId,
                TaskId,
                $"Your effort points for task '{task.Title}' have been updated to {request.AssignedEffortPoints} points by {assignerName}."
            );
        }

        public async Task MoveTaskToSprint(Guid projectId, Guid taskId, Guid? sprintId)
        {
            var task = await _taskProjectRepository.GetTaskByIdAsync(taskId);
            if (task == null || task.ProjectId != projectId)
            {
                throw new AppException(ErrorCode.TaskNotFound);
            }

            if (sprintId.HasValue)
            {
                var sprint = await _sprintRepository.GetSprintByIdAsync(sprintId.Value);
                if (sprint == null || sprint.ProjectId != projectId)
                {
                    throw new AppException(ErrorCode.SprintNotFound);
                }
            }

            task.SprintId = sprintId;
            task.UpdatedAt = _timeProvider.Now;
            await _taskProjectRepository.UpdateTaskAsync(task);

            // Log the movement
            var leader = await _projectMemberRepository.FindLeader(projectId);
            await _logService.LogTaskSprintChange(projectId, leader.Id, taskId, sprintId);
        }

        public async Task BulkMoveTasksToSprint(Guid projectId, List<Guid> taskIds, Guid? sprintId)
        {
            if (sprintId.HasValue)
            {
                var sprint = await _sprintRepository.GetSprintByIdAsync(sprintId.Value);
                if (sprint == null || sprint.ProjectId != projectId)
                {
                    throw new AppException(ErrorCode.SprintNotFound);
                }
            }

            var tasks = await _taskProjectRepository.GetListTasksByIdsAsync(taskIds);
            var validTasks = tasks.Where(t => t.ProjectId == projectId && t.IsActive).ToList();

            if (validTasks.Count != taskIds.Count)
            {
                throw new AppException(ErrorCode.SomeTasksNotFound);
            }

            foreach (var task in validTasks)
            {
                task.SprintId = sprintId;
                task.UpdatedAt = _timeProvider.Now;
            }

            await _taskProjectRepository.UpdateListTaskAsync(validTasks);

            // Log the bulk movement
            var leader = await _projectMemberRepository.FindLeader(projectId);
            await _logService.LogBulkTaskSprintChange(projectId, leader.Id, taskIds, sprintId);
        }
    }
}
