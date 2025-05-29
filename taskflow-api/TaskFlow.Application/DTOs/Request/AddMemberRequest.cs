using System.ComponentModel.DataAnnotations;

namespace taskflow_api.TaskFlow.Application.DTOs.Request
{
    public class AddMemberRequest
    {
        [Required(ErrorMessage = "ProjectId cannot be empty")]
        public Guid ProjectId { get; set; }

        [Required(ErrorMessage = "Email cannot be empty")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = null!;
    }
}
