using taskflow_api.TaskFlow.Domain.Common.Enums;

namespace taskflow_api.TaskFlow.Application.DTOs.Response
{
    public class SprintSummaryReportResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public SprintStatus Status { get; set; }
        
        // Task Statistics
        public int TotalTasksPlanned { get; set; }
        public int TasksCompleted { get; set; }
        public int TasksInProgress { get; set; }
        public int TasksNotStarted { get; set; }
        public int CarryoverTasks { get; set; }
        
        // Task Lists
        public List<TaskSummaryItem> CompletedTasks { get; set; } = new List<TaskSummaryItem>();
        public List<TaskSummaryItem> CarryoverTasksList { get; set; } = new List<TaskSummaryItem>();
        
        // Completion Rate
        public double CompletionRate => TotalTasksPlanned > 0 ? (double)TasksCompleted / TotalTasksPlanned * 100 : 0;
    }

    public class TaskSummaryItem
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;
        public TaskPriority Priority { get; set; }
        public DateTime Deadline { get; set; }
        public string Status { get; set; } = string.Empty;
        public List<string> Assignees { get; set; } = new List<string>();
    }
} 