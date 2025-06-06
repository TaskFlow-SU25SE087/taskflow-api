using System.ComponentModel.DataAnnotations;
using taskflow_api.TaskFlow.Application.DTOs.Common.Attributes;

namespace taskflow_api.TaskFlow.Application.DTOs.Request
{
    public class AddProfileUser
    {
        [Required(ErrorMessage = "Username is required.")]
        [StringLength(50, ErrorMessage = "Username must be between 8 and 50 characters.", MinimumLength = 8)]
        [NotEmail(ErrorMessage = "Username should not be the same as email.")]
        public string Username { get; set; } = null!;
        public IFormFile? Avatar { get; set; }
        public string? PhoneNumber { get; set; }
        [Required(ErrorMessage = "Gender name is required.")]
        public bool Gender  { get; set; }
    }
}
