using System.ComponentModel.DataAnnotations;

namespace taskflow_api.TaskFlow.Application.DTOs.Request
{
    public class UpdateAssigneeEffortPointsRequest
    {
        [Required(ErrorMessage = "Project member ID is required")]
        public Guid ProjectMemberId { get; set; }
        
        [Range(0, int.MaxValue, ErrorMessage = "Assigned effort points must be a positive number")]
        public int AssignedEffortPoints { get; set; }
    }
}
