using taskflow_api.TaskFlow.Domain.Common.Enums;

namespace taskflow_api.TaskFlow.Application.DTOs.Response
{
    public class TaskCompletionReportResponse
    {
        public Guid TaskId { get; set; }
        public string TaskTitle { get; set; } = string.Empty;
        public string? TaskDescription { get; set; }
        public TaskPriority Priority { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime Deadline { get; set; }
        public DateTime? CompletedAt { get; set; }
        public bool IsOverdue { get; set; }
        public TimeSpan? TimeSpent { get; set; }
        public string Status { get; set; } = string.Empty;
        public List<TaskAssigneeReportResponse> Assignees { get; set; } = new List<TaskAssigneeReportResponse>();
        public string? SprintName { get; set; }
        public string? BoardName { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
    }

    public class TaskAssigneeReportResponse
    {
        public Guid ProjectMemberId { get; set; }
        public string AssigneeName { get; set; } = string.Empty;
        public string? AssigneeAvatar { get; set; }
        public ProjectRole Role { get; set; }
        public DateTime AssignedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public TimeSpan? TimeSpent { get; set; }
    }

    public class TaskCompletionSummaryResponse
    {
        public int TotalTasks { get; set; }
        public int TodoTasks { get; set; }
        public int InProgressTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int OverdueTasks { get; set; }
        public double CompletionRate { get; set; }
        public TimeSpan TotalTimeSpent { get; set; }
        public List<TaskCompletionReportResponse> Tasks { get; set; } = new List<TaskCompletionReportResponse>();
    }
}
