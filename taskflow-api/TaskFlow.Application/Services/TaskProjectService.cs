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

            // Get assigner information for notification
            var assignerName = UserAssign.User?.FullName ?? UserAssign.User?.UserName ?? "Project Member";

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
            taskUpdate.Title = request.Title;
            taskUpdate.Description = request.Description;
            taskUpdate.Priority = request.Priority;
            taskUpdate.Deadline = request.Deadline;
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
            taskAssignee.UpdatedAt = _timeProvider.Now;
            taskAssignee.IsActive = false;
            taskAssignee.CancellationNote = string.IsNullOrWhiteSpace(reason)
                                            ? reason
                                            : defaultNote + ": " + reason;
            await _taskAssigneeRepository.UpdateAsync(taskAssignee);
        }

        public async Task<BurndownChartResponse> GetBurndownChart(Guid projectId, Guid sprintId)
        {
            // Get sprint information
            var sprint = await _sprintRepository.GetSprintByIdAsync(sprintId);
            if (sprint == null)
            {
                throw new AppException(ErrorCode.SprintNotFound);
            }

            if (sprint.ProjectId != projectId)
            {
                throw new AppException(ErrorCode.NoPermission);
            }

            // Get all tasks in the sprint
            var tasks = await _taskProjectRepository.GetTasksBySprintIdAsync(sprintId);
            
            // Calculate effort points by priority
            var priorityEfforts = new List<PriorityEffortData>();
            var totalEffortPoints = 0;
            var completedEffortPoints = 0;

            foreach (TaskPriority priority in Enum.GetValues(typeof(TaskPriority)))
            {
                var priorityTasks = tasks.Where(t => t.Priority == priority).ToList();
                var priorityTotalPoints = priorityTasks.Count * GetEffortPointsByPriority(priority);
                var priorityCompletedPoints = priorityTasks.Where(t => IsTaskCompleted(t)).Count() * GetEffortPointsByPriority(priority);

                priorityEfforts.Add(new PriorityEffortData
                {
                    Priority = priority,
                    PriorityName = priority.ToString(),
                    TotalEffortPoints = priorityTotalPoints,
                    CompletedEffortPoints = priorityCompletedPoints,
                    RemainingEffortPoints = priorityTotalPoints - priorityCompletedPoints,
                    CompletionPercentage = priorityTotalPoints > 0 ? (double)priorityCompletedPoints / priorityTotalPoints * 100 : 0
                });

                totalEffortPoints += priorityTotalPoints;
                completedEffortPoints += priorityCompletedPoints;
            }

            // Calculate daily progress
            var dailyProgress = new List<DailyProgressData>();
            var idealBurndown = new List<DailyProgressData>();
            var totalDays = (sprint.EndDate - sprint.StartDate).Days + 1;
            var dailyIdealBurn = totalEffortPoints / (double)totalDays;

            for (int i = 0; i <= totalDays; i++)
            {
                var currentDate = sprint.StartDate.AddDays(i);
                
                // Calculate cumulative completed tasks up to this date
                var completedTasksUpToDate = tasks.Where(t => 
                    IsTaskCompleted(t) && 
                    t.UpdatedAt.Date <= currentDate.Date).ToList();
                
                var completedPointsUpToDate = completedTasksUpToDate.Sum(t => GetEffortPointsByPriority(t.Priority));
                var remainingPoints = Math.Max(0, totalEffortPoints - completedPointsUpToDate);

                dailyProgress.Add(new DailyProgressData
                {
                    Date = currentDate,
                    RemainingEffortPoints = remainingPoints,
                    CompletedEffortPoints = completedPointsUpToDate,
                    TotalEffortPoints = totalEffortPoints
                });

                // Ideal burndown line (linear decrease)
                var idealRemaining = Math.Max(0, totalEffortPoints - (dailyIdealBurn * i));
                idealBurndown.Add(new DailyProgressData
                {
                    Date = currentDate,
                    RemainingEffortPoints = (int)idealRemaining,
                    CompletedEffortPoints = (int)(totalEffortPoints - idealRemaining),
                    TotalEffortPoints = totalEffortPoints
                });
            }

            return new BurndownChartResponse
            {
                SprintId = sprint.Id,
                SprintName = sprint.Name,
                StartDate = sprint.StartDate,
                EndDate = sprint.EndDate,
                TotalDays = totalDays,
                PriorityEfforts = priorityEfforts,
                DailyProgress = dailyProgress,
                IdealBurndown = idealBurndown
            };
        }

        private int GetEffortPointsByPriority(TaskPriority priority)
        {
            return priority switch
            {
                TaskPriority.Low => 1,
                TaskPriority.Medium => 3,
                TaskPriority.High => 5,
                TaskPriority.Urgent => 8,
                _ => 1
            };
        }

        private bool IsTaskCompleted(TaskProject task)
        {
            // Task is completed only when it has board type Done
            return task.Board != null && task.Board.Type == BoardType.Done;
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
    }
}
