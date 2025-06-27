using System.ComponentModel.DataAnnotations;

namespace taskflow_api.TaskFlow.Application.DTOs.Request
{
    public class RemoveAssignmentReasonRequest
    {
        [Required(ErrorMessage = "Assignee ID cannot be empty")]
        public Guid ImplementId { get; set; }
        [Required(ErrorMessage = "Reason cannot empty")]
        public string Reason { get; set; }

    }
}
