using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace taskflow_api.TaskFlow.Domain.Entities
{
    public class TaskComment
    {
        [Key]
        public Guid Id { get; set; }
        public Guid TaskId { get; set; }
        public TaskProject Task { get; set; } = null!;
        public Guid CommenterId { get; set; }
        public ProjectMember UserComment { get; set; } = null!;
        public DateTime CreateAt { get; set; } = DateTime.UtcNow;
        public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;
        [StringLength(500)]
        public string? Content { get; set; } = string.Empty;
        public string? AttachmentUrls { get; set; }
        [NotMapped]
        public List<string> AttachmentUrlsList
        {
            get => string.IsNullOrEmpty(AttachmentUrls)
                ? new List<string>()
                : AttachmentUrls.Split('|').ToList();
            set => AttachmentUrls = string.Join('|', value);
        }
    }
}
