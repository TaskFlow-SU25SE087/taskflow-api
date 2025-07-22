using System.ComponentModel.DataAnnotations;

namespace taskflow_api.TaskFlow.Domain.Entities
{
    public class GitMember
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public Guid ProjectPartId { get; set; }
        public ProjectPart ProjectPart { get; set; } = null!;

        public Guid? ProjectMemberId { get; set; }
        public ProjectMember? ProjectMember { get; set; } = null!;

        public string GitName { get; set; } = string.Empty;
        public string GitEmail { get; set; } = string.Empty;
        public string GitAvatarUrl { get; set; } = string.Empty;
        public string NameLocal { get; set; } = string.Empty;
        public string EmailLocal { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    }
}
