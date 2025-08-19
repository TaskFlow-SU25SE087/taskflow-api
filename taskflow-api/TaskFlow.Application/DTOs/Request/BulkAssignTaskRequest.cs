using System.ComponentModel.DataAnnotations;

namespace taskflow_api.TaskFlow.Application.DTOs.Request
{
    public class BulkAssignTaskRequest
    {
        [Required(ErrorMessage = "At least one assignee is required")]
        public List<TaskAssigneeRequest> Assignees { get; set; } = new List<TaskAssigneeRequest>();
    }

    public class TaskAssigneeRequest
    {
        [Required(ErrorMessage = "Implementer ID is required")]
        public Guid ImplementerId { get; set; }
        
        // Effort points assigned to this specific assignee
        [Range(0, int.MaxValue, ErrorMessage = "Assigned effort points must be a positive number")]
        public int? AssignedEffortPoints { get; set; }
    }
}

