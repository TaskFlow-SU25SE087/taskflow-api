using System.ComponentModel.DataAnnotations;

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

        [Required]
        public string title { get; set; } = string.Empty;
        public string description { get; set; } = string.Empty;
        public bool IsActive { get; set; } = false;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public List<TaskAssignee> TaskAssignees { get; set; }
        public Issue()
        {
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
