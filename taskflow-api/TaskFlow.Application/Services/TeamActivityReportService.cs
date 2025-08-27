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
        private readonly AppTimeProvider _timeProvider;

        public TeamActivityReportService(
            TaskFlowDbContext context,
            IProjectRepository projectRepository,
            IProjectMemberRepository projectMemberRepository,
            AppTimeProvider timeProvider)
        {
            _context = context;
            _projectRepository = projectRepository;
            _projectMemberRepository = projectMemberRepository;
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

            // Get tasks for this member
            var taskIds = taskAssignments.Select(ta => ta.RefId).ToList();
            var tasks = await _context.TaskProjects
                .Where(t => taskIds.Contains(t.Id) && t.ProjectId == projectId)
                .Include(t => t.Board)
                .Include(t => t.Sprint)
                .Where(t => t.CreatedAt >= startDate && t.CreatedAt <= endDate)
                .ToListAsync();

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
    }
}
