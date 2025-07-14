using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using taskflow_api.TaskFlow.Domain.Common.Enums;

namespace taskflow_api.TaskFlow.Domain.Entities
{
    public class TaskProject
    {
        [Key]
        public Guid Id { get; set; }
        public Guid? SprintId { get; set; }
        public Sprint? Sprint { get; set; } = null!;
        public Guid? BoardId { get; set; }
        public Board? Board { get; set; } 
        [Required]
        public Guid ProjectId { get; set; }
        public Project Project { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;
        [StringLength(100)]
        public string? Description { get; set; } = string.Empty;
        public TaskPriority Priority { get; set; } = TaskPriority.High;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? Note { get; set; }
        public string? AttachmentUrl { get; set; } = string.Empty;
        public string? CompletionAttachmentUrls { get; set; } = string.Empty;
        [NotMapped]
        public List<string> CompletionAttachmentUrlsList
        {
            get => string.IsNullOrEmpty(CompletionAttachmentUrls)
                ? new List<string>()
                : CompletionAttachmentUrls.Split('|').ToList();
            set => CompletionAttachmentUrls = string.Join('|', value);
        }
        public DateTime Deadline { get; set; } = DateTime.UtcNow.AddDays(7);
        public bool IsActive { get; set; } = false;
        public bool Deadline70Notified { get; set; } = false;
        public bool DeadlineExpiredNotified { get; set; } = false;

        public List<TaskAssignee> TaskAssignees { get; set; } = new List<TaskAssignee>();
        public List<Issue> Issues { get; set; } = new List<Issue>();
        public List<TaskTag> TaskTags { get; set; } = new List<TaskTag>();
        public List<TaskComment> TaskComments { get; set; } = new List<TaskComment>();
        public TaskProject()
        {
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
    }

}
