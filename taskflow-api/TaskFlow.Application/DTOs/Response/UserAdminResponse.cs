namespace taskflow_api.TaskFlow.Application.DTOs.Response
{
    public class UserAdminResponse
    {
        public Guid Id { get; set; }
        public string? Avatar { get; set; }
        public string FullName { get; set; } = null!;
        public string Role { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? StudentId { get; set; }
        public string? Term { get; set; }
        public bool IsActive { get; set; } = false;
    }
}
