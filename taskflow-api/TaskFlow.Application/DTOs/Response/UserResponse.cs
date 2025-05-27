namespace taskflow_api.TaskFlow.Application.DTOs.Response
{
    public class UserResponse
    {
        public Guid Id { get; set; }
        public string? Avatar { get; set; }
        public string FullName { get; set; } = null!;
        public string Role { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
    }
}
