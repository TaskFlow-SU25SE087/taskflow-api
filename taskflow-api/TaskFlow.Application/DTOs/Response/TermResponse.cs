using taskflow_api.TaskFlow.Domain.Entities;

namespace taskflow_api.TaskFlow.Application.DTOs.Response
{
    public class TermResponse
    {
        public Guid Id { get; set; } 
        public string Season { get; set; } = null!;
        public int Year { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; } = false;

        public List<UserResponseInTerm> Users { get; set; } = new();
    }

    public class UserResponseInTerm
    {
        public Guid Id { get; set; }
        public string? Avatar { get; set; }
        public string FullName { get; set; } = null!;
        public string Role { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? StudentId { get; set; }
        public string? TermSeason { get; set; }
        public int? TermYear { get; set; }
    }

}
