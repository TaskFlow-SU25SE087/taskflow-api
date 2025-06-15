using System.ComponentModel.DataAnnotations;

namespace taskflow_api.TaskFlow.Application.DTOs.Request
{
    public class AddAccountFileExcelRequest
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Full name is required.")]
        public string FullName { get; set; } = null!;

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 100 characters.")]
        public string Password { get; set; } = null!;
        [Required(ErrorMessage = "StudentId is required.")]
        public string StudentId { get; set; } = null!;
        [Required(ErrorMessage = "Term is required.")]
        public string Term { get; set; } = null!;
    }
}
