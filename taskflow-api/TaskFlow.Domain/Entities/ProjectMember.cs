using System.ComponentModel.DataAnnotations;
using taskflow_api.TaskFlow.Domain.Common.Enums;

namespace taskflow_api.TaskFlow.Domain.Entities
{
    public class ProjectMember
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public ProjectRole Role { get; set; }
        public Guid ProjectId { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        public bool IsActive { get; set; } = false;

        public List<TaskUser> taskUsers { get; set; } = new List<TaskUser>();
    }
}
