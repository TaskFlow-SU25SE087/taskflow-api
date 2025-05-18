using System.ComponentModel.DataAnnotations;

namespace taskflow_api.Model
{
    public class RegisterModel
    {
        [Required, EmailAddress]
        public string Email { get; set; } = null!;
        [Required]
        public string Password { get; set; } = null!;
        [Required]
        public string FullName { get; set; } = null!;
        [Required]
        public string ComfirmPassword { get; set; } = null!;
    }
}
