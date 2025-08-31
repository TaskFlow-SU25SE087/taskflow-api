using Microsoft.EntityFrameworkCore;
using taskflow_api.TaskFlow.Application.DTOs.Request;
using taskflow_api.TaskFlow.Application.DTOs.Response;
using taskflow_api.TaskFlow.Application.Interfaces;
using taskflow_api.TaskFlow.Domain.Common.Enums;
using taskflow_api.TaskFlow.Domain.Entities;
using taskflow_api.TaskFlow.Infrastructure.Data;
using taskflow_api.TaskFlow.Infrastructure.Interfaces;
using taskflow_api.TaskFlow.Shared.Exceptions;
using taskflow_api.TaskFlow.Shared.Helpers;

namespace taskflow_api.TaskFlow.Application.Services
{
    public class TeamActivityReportService : ITeamActivityReportService
    {
        private readonly TaskFlowDbContext _context;
        private readonly IProjectRepository _projectRepository;
        private readonly IProjectMemberRepository _projectMemberRepository;
        private readonly ISprintRepository _sprintRepository;
        private readonly ITaskProjectRepository _taskProjectRepository;
        private readonly AppTimeProvider _timeProvider;

        public TeamActivityReportService(
            TaskFlowDbContext context,
            IProjectRepository projectRepository,
            IProjectMemberRepository projectMemberRepository,
            ISprintRepository sprintRepository,
            ITaskProjectRepository taskProjectRepository,
            AppTimeProvider timeProvider)
        {
            _context = context;
            _projectRepository = projectRepository;
            _projectMemberRepository = projectMemberRepository;
            _sprintRepository = sprintRepository;
            _taskProjectRepository = taskProjectRepository;
            _timeProvider = timeProvider;
        }

        public async Task<TeamActivityReportResponse> GenerateTeamActivityReportAsync(Guid projectId, TeamActivityReportRequest request)
        {
            // Validate project exists
            var project = await _projectRepository.GetProjectByIdAsync(projectId)
                ?? throw new AppException(ErrorCode.ProjectNotFound);

            // Get all project members
            var memberResponses = await _projectMemberRepository.GetAllMembersInProjectAsync(projectId);
            
            // Filter members if specified
            if (request.MemberIds != null && request.MemberIds.Any())
            {
                memberResponses = memberResponses.Where(mr => request.MemberIds.Contains(mr.Id)).ToList();
            }

            // Get actual ProjectMember entities for the filtered members
            var projectMembers = new List<ProjectMember>();
            foreach (var memberResponse in memberResponses)
            {
                var projectMember = await _projectMemberRepository.FindMemberInProjectByProjectMemberID(memberResponse.Id);
                if (projectMember != null)
                {
                    projectMembers.Add(projectMember);
                }
            }

            var memberActivities = new List<MemberActivityResponse>();
            var totalTasks = 0;
            var totalCompletedTasks = 0;
            var totalInProgressTasks = 0;
            var totalTodoTasks = 0;
            var totalOverdueTasks = 0;
            var totalComments = 0;
            var totalAssignedEffortPoints = 0;
            var totalCompletedEffortPoints = 0;
            var totalInProgressEffortPoints = 0;
            var totalTodoEffortPoints = 0;
            var totalTasksWithEffortPoints = 0;
            var totalTasksWithoutEffortPoints = 0;

            foreach (var member in projectMembers)
            {
                var memberActivity = await GenerateMemberActivityReportAsync(projectId, member.UserId, request);
                memberActivities.Add(memberActivity);

                // Aggregate totals
                totalTasks += memberActivity.TaskStats.TotalAssigned;
                totalCompletedTasks += memberActivity.TaskStats.TotalCompleted;
                totalInProgressTasks += memberActivity.TaskStats.TotalInProgress;
                totalTodoTasks += memberActivity.TaskStats.TotalTodo;
                totalOverdueTasks += memberActivity.TaskStats.TotalOverdue;
                totalComments += memberActivity.CommentStats.TotalComments;
                
                // Aggregate effort point totals
                totalAssignedEffortPoints += memberActivity.EffortPointStats.TotalAssignedEffortPoints;
                totalCompletedEffortPoints += memberActivity.EffortPointStats.TotalCompletedEffortPoints;
                totalInProgressEffortPoints += memberActivity.EffortPointStats.TotalInProgressEffortPoints;
                totalTodoEffortPoints += memberActivity.EffortPointStats.TotalTodoEffortPoints;
                totalTasksWithEffortPoints += memberActivity.EffortPointStats.TotalTasksWithEffortPoints;
                totalTasksWithoutEffortPoints += memberActivity.EffortPointStats.TotalTasksWithoutEffortPoints;
            }

            // Calculate top contributors
            var topContributors = memberActivities
                .Select(ma => new TopContributor
                {
                    UserId = ma.UserId,
                    FullName = ma.FullName,
                    CompletedTasks = ma.TaskStats.TotalCompleted,
                    TotalComments = ma.CommentStats.TotalComments,
                    CompletedEffortPoints = ma.EffortPointStats.TotalCompletedEffortPoints,
                    ContributionScore = CalculateContributionScore(ma.TaskStats, ma.CommentStats, ma.EffortPointStats)
                })
                .OrderByDescending(tc => tc.ContributionScore)
                .Take(request.TopContributorsCount)
                .ToList();

            var summary = new TeamActivitySummary
            {
                TotalMembers = memberActivities.Count,
                TotalTasks = totalTasks,
                TotalCompletedTasks = totalCompletedTasks,
                TotalInProgressTasks = totalInProgressTasks,
                TotalTodoTasks = totalTodoTasks,
                TotalOverdueTasks = totalOverdueTasks,
                TotalComments = totalComments,
                OverallCompletionRate = totalTasks > 0 ? (double)totalCompletedTasks / totalTasks * 100 : 0,
                AverageTasksPerMember = memberActivities.Count > 0 ? (double)totalTasks / memberActivities.Count : 0,
                AverageCommentsPerTask = totalTasks > 0 ? (double)totalComments / totalTasks : 0,
                
                // Effort Point Summary
                TotalAssignedEffortPoints = totalAssignedEffortPoints,
                TotalCompletedEffortPoints = totalCompletedEffortPoints,
                TotalInProgressEffortPoints = totalInProgressEffortPoints,
                TotalTodoEffortPoints = totalTodoEffortPoints,
                OverallEffortPointCompletionRate = totalAssignedEffortPoints > 0 ? (double)totalCompletedEffortPoints / totalAssignedEffortPoints * 100 : 0,
                AverageEffortPointsPerTask = totalTasks > 0 ? (double)totalAssignedEffortPoints / totalTasks : 0,
                AverageEffortPointsPerMember = memberActivities.Count > 0 ? (double)totalAssignedEffortPoints / memberActivities.Count : 0,
                TotalTasksWithEffortPoints = totalTasksWithEffortPoints,
                TotalTasksWithoutEffortPoints = totalTasksWithoutEffortPoints,
                
                TopContributors = topContributors
            };

            return new TeamActivityReportResponse
            {
                ProjectId = projectId,
                ProjectTitle = project.Title,
                ReportGeneratedAt = _timeProvider.UtcNow,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                MemberActivities = memberActivities,
                Summary = summary
            };
        }

        public async Task<MemberActivityResponse> GenerateMemberActivityReportAsync(Guid projectId, Guid memberId, TeamActivityReportRequest request)
        {
            // Get project member
            var projectMember = await _projectMemberRepository.FindMemberInProject(projectId, memberId)
                ?? throw new AppException(ErrorCode.UserNotInProject);

            // Get user details
            var user = await _context.Users.FindAsync(memberId)
                ?? throw new AppException(ErrorCode.NoUserFound);

            // Build date filter
            var startDate = request.StartDate ?? DateTime.MinValue;
            var endDate = request.EndDate ?? _timeProvider.UtcNow;

            // Get task assignments for this member
            var taskAssignments = await _context.TaskAssignees
                .Where(ta => ta.ImplementerId == projectMember.Id && ta.IsActive)
                .Include(ta => ta.ProjectMember)
                .Include(ta => ta.ProjectMember.User)
                .ToListAsync();

            // ✅ FIXED: Include tasks that were either created OR completed within the date range
            // 
            // PROBLEM: Previous logic only filtered by CreatedAt, which excluded completed tasks
            // that were created outside the date range but completed within it.
            // 
            // SOLUTION: Include tasks that match ANY of these criteria:
            // 1. Created within the date range (original behavior)
            // 2. Completed (moved to Done status) within the date range  
            // 3. Assigned to the member within the date range
            // 
            // This ensures completed tasks are always counted correctly regardless of when they were created.
            var tasks = await _context.TaskProjects
                .Where(t => taskIds.Contains(t.Id) && t.ProjectId == projectId)
                .Include(t => t.Board)
                .Include(t => t.Sprint)
                .Where(t => 
                    // Task was created within the date range
                    (t.CreatedAt >= startDate && t.CreatedAt <= endDate) ||
                    // OR task was completed (moved to Done) within the date range
                    (t.Board.Type == BoardType.Done && t.UpdatedAt >= startDate && t.UpdatedAt <= endDate) ||
                    // OR task was assigned within the date range (for active assignments)
                    (taskAssignments.Any(ta => ta.RefId == t.Id && ta.CreatedAt >= startDate && ta.CreatedAt <= endDate))
                )
                .ToListAsync();

            // ✅ DEBUG: Log task inclusion for verification
            var createdTasks = tasks.Where(t => t.CreatedAt >= startDate && t.CreatedAt <= endDate).Count();
            var completedTasks = tasks.Where(t => t.Board?.Type == BoardType.Done && t.UpdatedAt >= startDate && t.UpdatedAt <= endDate).Count();
            var assignedTasks = tasks.Where(t => taskAssignments.Any(ta => ta.RefId == t.Id && ta.CreatedAt >= startDate && ta.CreatedAt <= endDate)).Count();
            
            // Log to console for debugging (remove in production)
            Console.WriteLine($"[DEBUG] Task filtering for member {memberId}:");
            Console.WriteLine($"  - Total tasks found: {tasks.Count}");
            Console.WriteLine($"  - Created in date range: {createdTasks}");
            Console.WriteLine($"  - Completed in date range: {completedTasks}");
            Console.WriteLine($"  - Assigned in date range: {assignedTasks}");
            Console.WriteLine($"  - Date range: {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}");

            // Get comments by this member
            var comments = await _context.TaskComments
                .Where(tc => tc.CommenterId == projectMember.Id)
                .Include(tc => tc.Task)
                .Where(tc => tc.CreateAt >= startDate && tc.CreateAt <= endDate)
                .ToListAsync();

            // Calculate task statistics
            var taskStats = CalculateTaskStats(tasks, taskAssignments);
            var commentStats = CalculateCommentStats(comments, tasks.Count);
            var effortPointStats = CalculateEffortPointStats(tasks, taskAssignments);

            // Get detailed task activities if requested
            var taskActivities = request.IncludeTaskDetails 
                ? await GetTaskActivityDetails(tasks, taskAssignments)
                : new List<TaskActivityDetail>();

            // Get detailed comment activities if requested
            var commentActivities = request.IncludeCommentDetails
                ? await GetCommentActivityDetails(comments)
                : new List<CommentActivityDetail>();

            return new MemberActivityResponse
            {
                UserId = user.Id,
                ProjectMemberId = projectMember.Id,
                FullName = user.FullName,
                Avatar = user.Avatar,
                Email = user.Email ?? "",
                Role = projectMember.Role,
                TaskStats = taskStats,
                CommentStats = commentStats,
                EffortPointStats = effortPointStats,
                TaskActivities = taskActivities,
                CommentActivities = commentActivities
            };
        }

        private TaskActivityStats CalculateTaskStats(List<TaskProject> tasks, List<TaskAssignee> taskAssignments)
        {
            var stats = new TaskActivityStats();
            var now = _timeProvider.UtcNow;

            foreach (var task in tasks)
            {
                stats.TotalAssigned++;

                // Count by status
                switch (task.Board?.Type)
                {
                    case BoardType.Done:
                        stats.TotalCompleted++;
                        break;
                    case BoardType.InProgress:
                        stats.TotalInProgress++;
                        break;
                    case BoardType.Todo:
                        stats.TotalTodo++;
                        break;
                }

                // Count by priority
                switch (task.Priority)
                {
                    case TaskPriority.High:
                        stats.HighPriorityTasks++;
                        break;
                    case TaskPriority.Medium:
                        stats.MediumPriorityTasks++;
                        break;
                    case TaskPriority.Low:
                        stats.LowPriorityTasks++;
                        break;
                    case TaskPriority.Urgent:
                        stats.UrgentPriorityTasks++;
                        break;
                }

                // Check if overdue
                if (task.Deadline.HasValue && task.Deadline.Value < now && task.Board?.Type != BoardType.Done)
                {
                    stats.TotalOverdue++;
                }
            }

            // Calculate completion rate
            stats.CompletionRate = stats.TotalAssigned > 0 
                ? (double)stats.TotalCompleted / stats.TotalAssigned * 100 
                : 0;

            return stats;
        }

        private CommentActivityStats CalculateCommentStats(List<TaskComment> comments, int totalTasks)
        {
            var stats = new CommentActivityStats();
            var now = _timeProvider.UtcNow;
            var weekAgo = now.AddDays(-7);
            var monthAgo = now.AddMonths(-1);

            stats.TotalComments = comments.Count;
            stats.CommentsThisWeek = comments.Count(c => c.CreateAt >= weekAgo);
            stats.CommentsThisMonth = comments.Count(c => c.CreateAt >= monthAgo);
            stats.LastCommentDate = comments.Any() ? comments.Max(c => c.CreateAt) : null;
            stats.AverageCommentsPerTask = totalTasks > 0 ? (double)comments.Count / totalTasks : 0;

            return stats;
        }

        private EffortPointStats CalculateEffortPointStats(List<TaskProject> tasks, List<TaskAssignee> taskAssignments)
        {
            var stats = new EffortPointStats();
            var totalEffortPoints = 0;
            var tasksWithEffortPoints = 0;
            var tasksWithoutEffortPoints = 0;

            foreach (var task in tasks)
            {
                var assignment = taskAssignments.FirstOrDefault(ta => ta.RefId == task.Id);
                var taskEffortPoints = task.EffortPoints ?? 0;
                var assignedEffortPoints = assignment?.AssignedEffortPoints ?? taskEffortPoints;

                if (assignedEffortPoints > 0)
                {
                    totalEffortPoints += assignedEffortPoints;
                    tasksWithEffortPoints++;

                    // Count effort points by status
                    switch (task.Board?.Type)
                    {
                        case BoardType.Done:
                            stats.TotalCompletedEffortPoints += assignedEffortPoints;
                            break;
                        case BoardType.InProgress:
                            stats.TotalInProgressEffortPoints += assignedEffortPoints;
                            break;
                        case BoardType.Todo:
                            stats.TotalTodoEffortPoints += assignedEffortPoints;
                            break;
                    }
                }
                else
                {
                    tasksWithoutEffortPoints++;
                }
            }

            stats.TotalAssignedEffortPoints = totalEffortPoints;
            stats.TotalTasksWithEffortPoints = tasksWithEffortPoints;
            stats.TotalTasksWithoutEffortPoints = tasksWithoutEffortPoints;
            stats.EffortPointCompletionRate = totalEffortPoints > 0 
                ? (double)stats.TotalCompletedEffortPoints / totalEffortPoints * 100 
                : 0;
            stats.AverageEffortPointsPerTask = tasks.Count > 0 ? (double)totalEffortPoints / tasks.Count : 0;

            return stats;
        }

        private async Task<List<TaskActivityDetail>> GetTaskActivityDetails(List<TaskProject> tasks, List<TaskAssignee> taskAssignments)
        {
            var details = new List<TaskActivityDetail>();
            var now = _timeProvider.UtcNow;

            foreach (var task in tasks)
            {
                var assignment = taskAssignments.FirstOrDefault(ta => ta.RefId == task.Id);
                if (assignment == null) continue;

                details.Add(new TaskActivityDetail
                {
                    TaskId = task.Id,
                    TaskTitle = task.Title,
                    Priority = task.Priority,
                    Status = task.Board?.Type ?? BoardType.Todo,
                    AssignedAt = assignment.CreatedAt,
                    CompletedAt = task.Board?.Type == BoardType.Done ? task.UpdatedAt : null,
                    Deadline = task.Deadline,
                    IsOverdue = task.Deadline.HasValue && task.Deadline.Value < now && task.Board?.Type != BoardType.Done,
                    SprintName = task.Sprint?.Name,
                    TaskEffortPoints = task.EffortPoints,
                    AssignedEffortPoints = assignment.AssignedEffortPoints
                });
            }

            return details.OrderByDescending(d => d.AssignedAt).ToList();
        }

        private async Task<List<CommentActivityDetail>> GetCommentActivityDetails(List<TaskComment> comments)
        {
            var details = new List<CommentActivityDetail>();

            foreach (var comment in comments)
            {
                details.Add(new CommentActivityDetail
                {
                    CommentId = comment.Id,
                    TaskId = comment.TaskId,
                    TaskTitle = comment.Task.Title,
                    Content = comment.Content ?? "",
                    CreatedAt = comment.CreateAt,
                    LastUpdatedAt = comment.LastUpdatedAt,
                    AttachmentUrls = comment.AttachmentUrlsList
                });
            }

            return details.OrderByDescending(d => d.CreatedAt).ToList();
        }

        private double CalculateContributionScore(TaskActivityStats taskStats, CommentActivityStats commentStats, EffortPointStats effortPointStats)
        {
            // Weighted scoring system
            var taskScore = taskStats.TotalCompleted * 10 + // Completed tasks worth 10 points each
                           taskStats.TotalInProgress * 5 +  // In-progress tasks worth 5 points each
                           taskStats.HighPriorityTasks * 3 + // High priority tasks worth 3 bonus points
                           taskStats.UrgentPriorityTasks * 5; // Urgent tasks worth 5 bonus points

            var commentScore = commentStats.TotalComments * 2; // Each comment worth 2 points

            var effortPointScore = effortPointStats.TotalCompletedEffortPoints * 2; // Each completed effort point worth 2 points

            var penaltyScore = taskStats.TotalOverdue * 5; // Overdue tasks penalty

            return Math.Max(0, taskScore + commentScore + effortPointScore - penaltyScore);
        }

        public async Task<BurndownChartResponse> GetBurndownChartAsync(Guid projectId, Guid sprintId)
        {
            // ✅ SECURITY: Additional input validation
            if (projectId == Guid.Empty || sprintId == Guid.Empty)
            {
                throw new AppException(ErrorCode.NoPermission);
            }

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
            
            // Calculate effort points by priority using actual task effort points
            var priorityEfforts = new List<PriorityEffortData>();
            var totalEffortPoints = 0;
            var completedEffortPoints = 0;

            foreach (TaskPriority priority in Enum.GetValues(typeof(TaskPriority)))
            {
                var priorityTasks = tasks.Where(t => t.Priority == priority).ToList();
                var priorityTotalPoints = priorityTasks.Sum(t => t.EffortPoints ?? 0);
                var priorityCompletedPoints = priorityTasks.Where(t => IsTaskCompleted(t)).Sum(t => t.EffortPoints ?? 0);

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
                
                var completedPointsUpToDate = completedTasksUpToDate.Sum(t => t.EffortPoints ?? 0);
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



        private bool IsTaskCompleted(TaskProject task)
        {
            // Task is completed only when it has board type Done
            return task.Board != null && task.Board.Type == BoardType.Done;
        }
    }
}
