using System.ComponentModel.DataAnnotations;
using taskflow_api.Enums;

namespace taskflow_api.Entity
{
    public class ProjectMember
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public ProjectRole Role { get; set; }
        public Guid ProjectId { get; set; }
        public string UserId { get; set; } = null!;
        public User User { get; set; } = null!;
        public bool IsActive { get; set; } = false;

        public List<TaskUser> taskUsers { get; set; } = new List<TaskUser>();
    }
}
