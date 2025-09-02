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

        /// <summary>
        /// Generate a comprehensive team activity report for all members in a project
        /// This report shows GLOBAL project metrics (not sprint-specific) to give a complete project overview
        /// </summary>
        public async Task<TeamActivityReportResponse> GenerateTeamActivityReportAsync(Guid projectId, TeamActivityReportRequest request)
        {
            try
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
            
            // ✅ FIXED: Calculate team-level metrics directly from project data to avoid double-counting
            // This prevents issues where tasks with multiple assignees would be counted multiple times
            // in the team totals, leading to inflated metrics.
            
            // ✅ MODIFIED: Team activity report now shows GLOBAL project metrics (not sprint-specific)
            // Date filtering is still applied for historical analysis, but all project tasks are included
            var startDate = request.StartDate ?? DateTime.MinValue;
            var endDate = request.EndDate ?? _timeProvider.UtcNow;
            
            var allProjectTasks = await _context.TaskProjects
                .Where(t => t.ProjectId == projectId && t.IsActive)
                .Include(t => t.Board)
                .Include(t => t.Sprint)
                .Include(t => t.TaskAssignees.Where(ta => ta.IsActive))
                .ToListAsync();

            // Filter tasks by date range for historical analysis, but include ALL project tasks
            // This gives a complete view of the project regardless of sprint assignments
            var projectTasks = allProjectTasks.Where(t => 
                (t.CreatedAt >= startDate && t.CreatedAt <= endDate) ||
                (t.Board?.Type == BoardType.Done && t.UpdatedAt >= startDate && t.UpdatedAt <= endDate) ||
                (t.TaskAssignees.Any(ta => ta.CreatedAt >= startDate && ta.CreatedAt <= endDate))
            ).ToList();

            // Calculate team-level metrics
            var totalTasks = projectTasks.Count;
            var totalCompletedTasks = projectTasks.Count(t => t.Board?.Type == BoardType.Done);
            var totalInProgressTasks = projectTasks.Count(t => t.Board?.Type == BoardType.InProgress);
            var totalTodoTasks = projectTasks.Count(t => t.Board?.Type == BoardType.Todo);
            var totalOverdueTasks = projectTasks.Count(t => 
                t.Deadline.HasValue && t.Deadline.Value < _timeProvider.UtcNow && t.Board?.Type != BoardType.Done);

            // Calculate team-level effort points
            var totalAssignedEffortPoints = projectTasks.Sum(t => t.EffortPoints ?? 0);
            var totalCompletedEffortPoints = projectTasks
                .Where(t => t.Board?.Type == BoardType.Done)
                .Sum(t => t.EffortPoints ?? 0);
            var totalInProgressEffortPoints = projectTasks
                .Where(t => t.Board?.Type == BoardType.InProgress)
                .Sum(t => t.EffortPoints ?? 0);
            var totalTodoEffortPoints = projectTasks
                .Where(t => t.Board?.Type == BoardType.Todo)
                .Sum(t => t.EffortPoints ?? 0);

            var totalTasksWithEffortPoints = projectTasks.Count(t => (t.EffortPoints ?? 0) > 0);
            var totalTasksWithoutEffortPoints = projectTasks.Count(t => (t.EffortPoints ?? 0) == 0);

            // Get total comments for the project in the date range
            var totalComments = await _context.TaskComments
                .Where(tc => tc.Task.ProjectId == projectId && 
                            tc.CreateAt >= startDate && tc.CreateAt <= endDate)
                .CountAsync();

            // ✅ DEBUG: Log team-level metrics for verification
            Console.WriteLine($"[DEBUG] Team-level metrics for project {projectId}:");
            Console.WriteLine($"  - Total tasks: {totalTasks}");
            Console.WriteLine($"  - Completed tasks: {totalCompletedTasks}");
            Console.WriteLine($"  - In-progress tasks: {totalInProgressTasks}");
            Console.WriteLine($"  - Todo tasks: {totalTodoTasks}");
            Console.WriteLine($"  - Overdue tasks: {totalOverdueTasks}");
            Console.WriteLine($"  - Total effort points: {totalAssignedEffortPoints}");
            Console.WriteLine($"  - Completed effort points: {totalCompletedEffortPoints}");
            Console.WriteLine($"  - Total comments: {totalComments}");

            foreach (var member in projectMembers)
            {
                var memberActivity = await GenerateMemberActivityReportAsync(projectId, member.UserId, request);
                memberActivities.Add(memberActivity);
            }



            // ✅ FIXED: Calculate top contributors based on individual member metrics (not team totals)
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
                
                // ✅ FIXED: Effort Point Summary - now using accurate team-level calculations
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

            // ✅ VALIDATION: Ensure team metrics are consistent
            ValidateTeamMetrics(memberActivities, summary);

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
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] GenerateTeamActivityReportAsync failed for project {projectId}: {ex.Message}");
                Console.WriteLine($"[ERROR] Stack trace: {ex.StackTrace}");
                throw; // Re-throw to maintain original error handling
            }
        }

        /// <summary>
        /// Generate an activity report for a specific member in a project
        /// This report shows GLOBAL project metrics for the member (not sprint-specific)
        /// </summary>
        public async Task<MemberActivityResponse> GenerateMemberActivityReportAsync(Guid projectId, Guid memberId, TeamActivityReportRequest request)
        {
            try
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
            Console.WriteLine($"[DEBUG] Getting task assignments for member {memberId} (ProjectMemberId: {projectMember.Id})");
            var taskAssignments = await _context.TaskAssignees
                .Where(ta => ta.ImplementerId == projectMember.Id && ta.IsActive)
                .Include(ta => ta.ProjectMember)
                .Include(ta => ta.ProjectMember.User)
                .ToListAsync();
            Console.WriteLine($"[DEBUG] Found {taskAssignments.Count} task assignments");

            // Get task IDs from assignments (distinct to avoid duplicates)
            var taskIds = taskAssignments.Select(ta => ta.RefId).Distinct().ToList();
            
            // ✅ SAFETY: Handle case where member has no task assignments
            if (!taskIds.Any())
            {
                Console.WriteLine($"[DEBUG] Member {memberId} has no task assignments, returning empty report");
                return new MemberActivityResponse
                {
                    UserId = user.Id,
                    ProjectMemberId = projectMember.Id,
                    FullName = user.FullName,
                    Avatar = user.Avatar,
                    Email = user.Email ?? "",
                    Role = projectMember.Role,
                    TaskStats = new TaskActivityStats(),
                    CommentStats = new CommentActivityStats(),
                    EffortPointStats = new EffortPointStats(),
                    TaskActivities = new List<TaskActivityDetail>(),
                    CommentActivities = new List<CommentActivityDetail>()
                };
            }

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
            // First, get all tasks for this member
            Console.WriteLine($"[DEBUG] Getting tasks for {taskIds.Count} task IDs");
            var allTasks = await _context.TaskProjects
                .Where(t => taskIds.Contains(t.Id) && t.ProjectId == projectId)
                .Include(t => t.Board)
                .Include(t => t.Sprint)
                .ToListAsync();
            Console.WriteLine($"[DEBUG] Found {allTasks.Count} tasks in database");

            // Then filter in memory to avoid LINQ translation issues
            var tasks = allTasks.Where(t => 
                // Task was created within the date range
                (t.CreatedAt >= startDate && t.CreatedAt <= endDate) ||
                // OR task was completed (moved to Done) within the date range
                (t.Board?.Type == BoardType.Done && t.UpdatedAt >= startDate && t.UpdatedAt <= endDate) ||
                // OR task was assigned within the date range (for active assignments)
                (taskAssignments.Any(ta => ta.RefId == t.Id && ta.CreatedAt >= startDate && ta.CreatedAt <= endDate))
            ).ToList();

            // ✅ SAFETY: Ensure we have valid data
            if (tasks == null) tasks = new List<TaskProject>();
            if (taskAssignments == null) taskAssignments = new List<TaskAssignee>();

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
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] GenerateMemberActivityReportAsync failed for member {memberId}: {ex.Message}");
                Console.WriteLine($"[ERROR] Stack trace: {ex.StackTrace}");
                throw; // Re-throw to maintain original error handling
            }
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
                
                // ✅ FIXED: Use assigned effort points if available, otherwise fall back to task effort points
                // This ensures we don't double-count effort points when multiple assignees exist
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

        /// <summary>
        /// Get burndown chart data for a specific sprint in a project
        /// This method remains sprint-specific as requested - only the team activity report is global
        /// </summary>
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

            // ✅ FIXED: Validate sprint dates
            if (sprint.EndDate <= sprint.StartDate)
            {
                throw new AppException(ErrorCode.InvalidSprintDates);
            }

            // Get all tasks in the sprint
            var tasks = await _taskProjectRepository.GetTasksBySprintIdAsync(sprintId);
            
            // ✅ FIXED: Calculate date variables early to avoid scope issues
            var startDate = sprint.StartDate.Date;
            var endDate = sprint.EndDate.Date;
            var totalDays = (endDate - startDate).Days + 1;
            
            // ✅ FIXED: Add validation for empty sprint
            if (!tasks.Any())
            {
                Console.WriteLine($"[BURNDOWN_DEBUG] No tasks found in sprint {sprint.Name}");
                // Return empty burndown chart for sprints without tasks
                return new BurndownChartResponse
                {
                    SprintId = sprint.Id,
                    SprintName = sprint.Name,
                    StartDate = startDate,
                    EndDate = endDate,
                    TotalDays = totalDays,
                    PriorityEfforts = new List<PriorityEffortData>(),
                    DailyProgress = new List<DailyProgressData>(),
                    IdealBurndown = new List<DailyProgressData>()
                };
            }
            
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

            // ✅ FIXED: Calculate daily progress with improved date handling
            var dailyProgress = new List<DailyProgressData>();
            var idealBurndown = new List<DailyProgressData>();
            
            // Ensure we're working with date-only values to avoid timezone issues
            // Note: startDate, endDate, and totalDays are already calculated above
            
            // ✅ FIXED: Prevent division by zero and handle edge cases
            if (totalDays <= 0)
            {
                throw new AppException(ErrorCode.InvalidSprintDates);
            }

            var dailyIdealBurn = totalEffortPoints / (double)totalDays;
            
            // ✅ FIXED: Add debugging information
            Console.WriteLine($"[BURNDOWN_DEBUG] Sprint: {sprint.Name}");
            Console.WriteLine($"[BURNDOWN_DEBUG] Start Date: {startDate:yyyy-MM-dd}, End Date: {endDate:yyyy-MM-dd}");
            Console.WriteLine($"[BURNDOWN_DEBUG] Total Days: {totalDays}, Total Effort Points: {totalEffortPoints}");
            Console.WriteLine($"[BURNDOWN_DEBUG] Daily Ideal Burn: {dailyIdealBurn:F2}");

            for (int i = 0; i <= totalDays; i++)
            {
                var currentDate = startDate.AddDays(i);
                
                // Calculate cumulative completed tasks up to this date
                var completedTasksUpToDate = tasks.Where(t => 
                    IsTaskCompleted(t) && 
                    t.UpdatedAt.Date <= currentDate.Date).ToList();
                
                var completedPointsUpToDate = completedTasksUpToDate.Sum(t => t.EffortPoints ?? 0);
                var remainingPoints = Math.Max(0, totalEffortPoints - completedPointsUpToDate);

                // ✅ FIXED: Add daily debugging information
                if (i % 7 == 0 || i == totalDays) // Log every week and the last day
                {
                    Console.WriteLine($"[BURNDOWN_DEBUG] Day {i}: {currentDate:yyyy-MM-dd} - Completed: {completedPointsUpToDate}, Remaining: {remainingPoints}");
                }

                dailyProgress.Add(new DailyProgressData
                {
                    Date = currentDate,
                    RemainingEffortPoints = remainingPoints,
                    CompletedEffortPoints = completedPointsUpToDate,
                    TotalEffortPoints = totalEffortPoints
                });

                // ✅ FIXED: Improved ideal burndown line calculation
                var idealRemaining = Math.Max(0, totalEffortPoints - (dailyIdealBurn * i));
                idealBurndown.Add(new DailyProgressData
                {
                    Date = currentDate,
                    RemainingEffortPoints = (int)Math.Round(idealRemaining), // Use Math.Round instead of casting
                    CompletedEffortPoints = (int)Math.Round(totalEffortPoints - idealRemaining),
                    TotalEffortPoints = totalEffortPoints
                });
            }

            return new BurndownChartResponse
            {
                SprintId = sprint.Id,
                SprintName = sprint.Name,
                StartDate = startDate, // Use normalized date
                EndDate = endDate,     // Use normalized date
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

        // ✅ NEW: Helper method to get accurate task completion date
        private DateTime GetTaskCompletionDate(TaskProject task)
        {
            // If task has a completion date field, use it
            // Otherwise, use UpdatedAt as fallback
            // You might want to add a CompletionDate field to TaskProject entity in the future
            
            // For now, we'll use UpdatedAt as a proxy for completion date
            // In the future, consider adding a dedicated CompletionDate field to TaskProject
            // or tracking board transitions in a separate audit table
            
            var completionDate = task.UpdatedAt.Date;
            
            // Log for debugging purposes
            Console.WriteLine($"[BURNDOWN_DEBUG] Task '{task.Title}' completion date: {completionDate}, Board: {task.Board?.Name ?? "No Board"}");
            
            return completionDate;
        }

        /// <summary>
        /// Validates that team-level metrics are consistent and don't exceed individual member totals
        /// </summary>
        private void ValidateTeamMetrics(List<MemberActivityResponse> memberActivities, TeamActivitySummary summary)
        {
            var totalMemberTasks = memberActivities.Sum(ma => ma.TaskStats.TotalAssigned);
            var totalMemberEffortPoints = memberActivities.Sum(ma => ma.EffortPointStats.TotalAssignedEffortPoints);
            
            // Log validation results
            Console.WriteLine($"[VALIDATION] Team metrics validation:");
            Console.WriteLine($"  - Team total tasks: {summary.TotalTasks}, Sum of member tasks: {totalMemberTasks}");
            Console.WriteLine($"  - Team total effort points: {summary.TotalAssignedEffortPoints}, Sum of member effort points: {totalMemberEffortPoints}");
            
            // Warn if there are significant discrepancies (this could indicate double-counting issues)
            if (totalMemberTasks > summary.TotalTasks * 1.5) // Allow 50% tolerance for edge cases
            {
                Console.WriteLine($"[WARNING] Member task total ({totalMemberTasks}) significantly exceeds team total ({summary.TotalTasks})");
            }
            
            if (totalMemberEffortPoints > summary.TotalAssignedEffortPoints * 1.5)
            {
                Console.WriteLine($"[WARNING] Member effort point total ({totalMemberEffortPoints}) significantly exceeds team total ({summary.TotalAssignedEffortPoints})");
            }
        }
    }
}
