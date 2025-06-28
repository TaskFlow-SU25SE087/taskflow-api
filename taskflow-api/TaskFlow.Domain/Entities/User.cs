using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using taskflow_api.TaskFlow.Domain.Common.Enums;

namespace taskflow_api.TaskFlow.Domain.Entities
{
    public class User : IdentityUser<Guid>
    {
        public string? Avatar { get; set; } = "https://res.cloudinary.com/dpw9sgxab/image/upload/v1749247007/avatar/default.jpg";
        public string FullName { get; set; } = null!;
        public string? StudentId { get; set; }
        public string? TermSeason { get; set; }
        public int TermYear { get; set; } = 0;
        public string? PastTerms { get; set; }
        public bool IsActive { get; set; } = false;

        public UserRole Role { get; set; } = UserRole.User;

        public List<ProjectMember> ProjectMembers { get; set; } = new();
        public List<UserBans> Bans { get; set; } = new();
        public List<UserReports> Reports { get; set; } = new();
        public List<UserAppeals> Appeals { get; set; } = new();

    }
}
