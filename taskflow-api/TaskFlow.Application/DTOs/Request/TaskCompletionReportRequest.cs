using System.ComponentModel;
using taskflow_api.TaskFlow.Domain.Common.Enums;

namespace taskflow_api.TaskFlow.Application.DTOs.Request
{
    /// <summary>
    /// Optional filter parameters for task completion report
    /// Note: All parameters are optional - only projectId is required in the URL
    /// </summary>
    public class TaskCompletionReportRequest
    {
        /// <summary>
        /// Optional: Filter tasks by specific sprint
        /// </summary>
        [Description("Optional: Filter tasks by specific sprint")]
        public Guid? SprintId { get; set; }

        /// <summary>
        /// Optional: Filter tasks by status (Todo, InProgress, Done)
        /// </summary>
        [Description("Optional: Filter tasks by status")]
        public BoardType? Status { get; set; }

        /// <summary>
        /// Optional: Filter tasks assigned to specific team member
        /// </summary>
        [Description("Optional: Filter tasks by assignee")]
        public Guid? AssigneeId { get; set; }

        /// <summary>
        /// Optional: Filter tasks created after this date
        /// </summary>
        [Description("Optional: Filter tasks created after this date")]
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Optional: Filter tasks created before this date
        /// </summary>
        [Description("Optional: Filter tasks created before this date")]
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Optional: Filter by overdue status (true = overdue, false = not overdue)
        /// </summary>
        [Description("Optional: Filter by overdue status")]
        public bool? IsOverdue { get; set; }

        /// <summary>
        /// Optional: Filter tasks by priority level
        /// </summary>
        [Description("Optional: Filter tasks by priority")]
        public TaskPriority? Priority { get; set; }

        /// <summary>
        /// Optional: Include completed tasks in results (default: true)
        /// </summary>
        [Description("Optional: Include completed tasks")]
        public bool IncludeCompleted { get; set; } = true;

        /// <summary>
        /// Optional: Include in-progress tasks in results (default: true)
        /// </summary>
        [Description("Optional: Include in-progress tasks")]
        public bool IncludeInProgress { get; set; } = true;

        /// <summary>
        /// Optional: Include todo tasks in results (default: true)
        /// </summary>
        [Description("Optional: Include todo tasks")]
        public bool IncludeTodo { get; set; } = true;
    }
}
