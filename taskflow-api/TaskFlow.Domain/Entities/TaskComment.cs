using System.ComponentModel.DataAnnotations;

namespace taskflow_api.TaskFlow.Domain.Entities
{
    public class TaskComment
    {
        [Key]
        public Guid Id { get; set; }
        public Guid TaskId { get; set; }
        public TaskProject Task { get; set; } = null!;
        public Guid Commenter { get; set; }
        public ProjectMember User { get; set; } = null!;
        public DateTime CreateAt { get; set; } = DateTime.UtcNow;
        public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;
        [StringLength(500)]
        public string? Content { get; set; } = string.Empty;
    }
}
