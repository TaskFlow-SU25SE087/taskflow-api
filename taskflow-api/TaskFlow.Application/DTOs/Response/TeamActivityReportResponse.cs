using taskflow_api.TaskFlow.Domain.Common.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace taskflow_api.TaskFlow.Application.DTOs.Response
{
    public class TeamActivityReportResponse
    {
        public Guid ProjectId { get; set; }
        public string ProjectTitle { get; set; } = string.Empty;
        public DateTime ReportGeneratedAt { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<MemberActivityResponse> MemberActivities { get; set; } = new();
        public TeamActivitySummary Summary { get; set; } = new();
    }

    public class MemberActivityResponse
    {
        public Guid UserId { get; set; }
        public Guid ProjectMemberId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Avatar { get; set; }
        public string Email { get; set; } = string.Empty;
        public ProjectRole Role { get; set; }
        public TaskActivityStats TaskStats { get; set; } = new();
        public CommentActivityStats CommentStats { get; set; } = new();
        public EffortPointStats EffortPointStats { get; set; } = new();
        public List<TaskActivityDetail> TaskActivities { get; set; } = new();
        public List<CommentActivityDetail> CommentActivities { get; set; } = new();
    }

    public class TaskActivityStats
    {
        public int TotalAssigned { get; set; }
        public int TotalCompleted { get; set; }
        public int TotalInProgress { get; set; }
        public int TotalTodo { get; set; }
        public int TotalOverdue { get; set; }
        public double CompletionRate { get; set; }
        public int HighPriorityTasks { get; set; }
        public int MediumPriorityTasks { get; set; }
        public int LowPriorityTasks { get; set; }
        public int UrgentPriorityTasks { get; set; }
    }

    public class CommentActivityStats
    {
        public int TotalComments { get; set; }
        public int CommentsThisWeek { get; set; }
        public int CommentsThisMonth { get; set; }
        public DateTime? LastCommentDate { get; set; }
        public double AverageCommentsPerTask { get; set; }
    }

    public class EffortPointStats
    {
        public int TotalAssignedEffortPoints { get; set; }
        public int TotalCompletedEffortPoints { get; set; }
        public int TotalInProgressEffortPoints { get; set; }
        public int TotalTodoEffortPoints { get; set; }
        public double EffortPointCompletionRate { get; set; }
        public double AverageEffortPointsPerTask { get; set; }
        public int TotalTasksWithEffortPoints { get; set; }
        public int TotalTasksWithoutEffortPoints { get; set; }
    }

    public class TaskActivityDetail
    {
        public Guid TaskId { get; set; }
        public string TaskTitle { get; set; } = string.Empty;
        [JsonConverter(typeof(StringEnumConverter))]
        public TaskPriority Priority { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public BoardType Status { get; set; }
        public DateTime AssignedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? Deadline { get; set; }
        public bool IsOverdue { get; set; }
        public string? SprintName { get; set; }
        public int? TaskEffortPoints { get; set; }
        public int? AssignedEffortPoints { get; set; }
    }

    public class CommentActivityDetail
    {
        public Guid CommentId { get; set; }
        public Guid TaskId { get; set; }
        public string TaskTitle { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdatedAt { get; set; }
        public List<string> AttachmentUrls { get; set; } = new();
    }

    public class TeamActivitySummary
    {
        public int TotalMembers { get; set; }
        public int TotalTasks { get; set; }
        public int TotalCompletedTasks { get; set; }
        public int TotalInProgressTasks { get; set; }
        public int TotalTodoTasks { get; set; }
        public int TotalOverdueTasks { get; set; }
        public int TotalComments { get; set; }
        public double OverallCompletionRate { get; set; }
        public double AverageTasksPerMember { get; set; }
        public double AverageCommentsPerTask { get; set; }
        
        // Effort Point Summary
        public int TotalAssignedEffortPoints { get; set; }
        public int TotalCompletedEffortPoints { get; set; }
        public int TotalInProgressEffortPoints { get; set; }
        public int TotalTodoEffortPoints { get; set; }
        public double OverallEffortPointCompletionRate { get; set; }
        public double AverageEffortPointsPerTask { get; set; }
        public double AverageEffortPointsPerMember { get; set; }
        public int TotalTasksWithEffortPoints { get; set; }
        public int TotalTasksWithoutEffortPoints { get; set; }
        
        public List<TopContributor> TopContributors { get; set; } = new();
    }

    public class TopContributor
    {
        public Guid UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public int CompletedTasks { get; set; }
        public int TotalComments { get; set; }
        public int CompletedEffortPoints { get; set; }
        public double ContributionScore { get; set; }
    }
}
