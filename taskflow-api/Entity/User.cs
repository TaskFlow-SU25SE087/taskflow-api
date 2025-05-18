using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace taskflow_api.Entity
{
    public class User : IdentityUser
    {
        public string? Avatar { get; set; }
        public string FullName { get; set; } = null!;
        public bool IsActive { get; set; } = false;

        public Enums.UserRole Role { get; set; } = Enums.UserRole.User;

        public List<ProjectMember> ProjectMembers { get; set; } = new();

    }
}
