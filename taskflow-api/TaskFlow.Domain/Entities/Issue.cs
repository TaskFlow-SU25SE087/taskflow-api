using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using taskflow_api.TaskFlow.Domain.Common.Enums;

namespace taskflow_api.TaskFlow.Domain.Entities
{
    public class Issue
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid? TaskProjectID { get; set; }
        public TaskProject TaskProject { get; set; } = null!;
        [Required]
        public Guid ProjectId { get; set; }
        public Project Project { get; set; }
        public Guid CreatedBy { get; set; }
        [Required]
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? Explanation { get; set; } = string.Empty;
        public string? Example { get; set; } = string.Empty;
        public TaskPriority Priority { get; set; }
        public bool IsActive { get; set; } = false;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public TypeIssue Type { get; set; }
        public IssueStatus Status { get; set; } = IssueStatus.Open;
        public string? IssueAttachmentUrls { get; set; } = string.Empty;
        [NotMapped]
        public List<string> IssueAttachmentUrlsList
        {
            get => string.IsNullOrEmpty(IssueAttachmentUrls)
                ? new List<string>()
                : IssueAttachmentUrls.Split('|').ToList();
            set => IssueAttachmentUrls = string.Join('|', value);
        }

        public List<TaskAssignee> TaskAssignees { get; set; }
        public Issue()
        {
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
