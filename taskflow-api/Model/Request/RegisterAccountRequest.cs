using System.ComponentModel.DataAnnotations;

namespace taskflow_api.Model.Request
{
    public class RegisterAccountRequest
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Full name is required.")]
        public string FullName { get; set; } = null!;

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 100 characters.")]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "Confirm password is required.")]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = null!;

        public IFormFile? Avatar { get; set; }
    }
}
