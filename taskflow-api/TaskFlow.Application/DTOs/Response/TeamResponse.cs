using taskflow_api.TaskFlow.Domain.Common.Enums;

namespace taskflow_api.TaskFlow.Application.DTOs.Response
{
    public class TeamResponse
    {
        public Guid ProjectId { get; set; }
        public string ProjectTitle { get; set; } = string.Empty;
        public string ProjectDescription { get; set; } = string.Empty;
        public string Semester { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdate { get; set; }
        public bool IsActive { get; set; }
        public List<TeamMemberResponse> Members { get; set; } = new List<TeamMemberResponse>();
        public int TotalMembers { get; set; }
        public int ActiveMembers { get; set; }
    }

    public class TeamMemberResponse
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Avatar { get; set; }
        public string? StudentId { get; set; }
        public ProjectRole Role { get; set; }
        public bool IsActive { get; set; }
        public bool HasJoinedBefore { get; set; }
    }
}
