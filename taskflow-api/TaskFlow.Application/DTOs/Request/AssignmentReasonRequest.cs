using System.ComponentModel.DataAnnotations;

namespace taskflow_api.TaskFlow.Application.DTOs.Request
{
    public class AssignmentReasonRequest
    {
        [Required(ErrorMessage = "Reason cannot empty")]
        public string Reason { get; set; } 
    }
}
