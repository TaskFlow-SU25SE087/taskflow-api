using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using taskflow_api.TaskFlow.Domain.Common.Enums;

namespace taskflow_api.TaskFlow.Domain.Entities
{
    public class User : IdentityUser<Guid>
    {
        public string? Avatar { get; set; } = "Image/Avatars/avatar.png";
        public string FullName { get; set; } = null!;
        public bool? Gender { get; set; }
        public bool IsBanned { get; set; } = false;
        public bool IsPermanentlyBanned { get; set; } = false;
        public int ReportCount { get; set; } = 0;

        public UserRole Role { get; set; } = UserRole.User;

        public List<ProjectMember> ProjectMembers { get; set; } = new();
        public List<UserBans> Bans { get; set; } = new();
        public List<UserReports> Reports { get; set; } = new();
        public List<UserAppeals> Appeals { get; set; } = new();

    }
}
