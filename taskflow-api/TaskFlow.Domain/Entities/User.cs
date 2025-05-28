using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using taskflow_api.TaskFlow.Domain.Common.Enums;

namespace taskflow_api.TaskFlow.Domain.Entities
{
    public class User : IdentityUser<Guid>
    {
        public string? Avatar { get; set; } = "Image/Avatars/avatar.png";
        public string FullName { get; set; } = null!;
        public bool IsActive { get; set; } = false;

        public UserRole Role { get; set; } = UserRole.User;

        public List<ProjectMember> ProjectMembers { get; set; } = new();

    }
}
