using taskflow_api.TaskFlow.Domain.Common.Enums;

namespace taskflow_api.TaskFlow.Application.DTOs.Response
{
    public class UserResponse
    {
        public Guid Id { get; set; }
        public string? Avatar { get; set; }
        public string FullName { get; set; } = null!;
        public UserRole Role { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? StudentId { get; set; }
        public string? Season { get; set; }
        public int? Year { get; set; }
        public string? PastTerms { get; set; }
    }
}
