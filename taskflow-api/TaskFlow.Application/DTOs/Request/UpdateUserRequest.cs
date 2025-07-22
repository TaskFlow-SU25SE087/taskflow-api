using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace taskflow_api.TaskFlow.Application.DTOs.Request
{
    public class UpdateUserRequest
    {

        [Required(ErrorMessage = "Full name is required.")]
        public string FullName { get; set; } = null!;

        public String? PhoneNumber { get; set; }
        public IFormFile? Avatar { get; set; }

    }
}
