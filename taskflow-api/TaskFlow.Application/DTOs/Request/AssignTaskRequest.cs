using System.ComponentModel.DataAnnotations;

namespace taskflow_api.TaskFlow.Application.DTOs.Request
{
    public class AssignTaskRequest
    {
        public Guid ImplementerId { get; set; }
        
        // Effort points assigned to this specific assignee
        [Range(0, int.MaxValue, ErrorMessage = "Assigned effort points must be a positive number")]
        public int? AssignedEffortPoints { get; set; }
    }
}
