using System.ComponentModel.DataAnnotations;
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
        public Board? Board { get; set; } = null!;
        [Required]
        public Guid ProjectId { get; set; }
        public Project Project { get; set; } = null!;

        [Required]
        public string Title { get; set; } = string.Empty;
        [StringLength(100)]
        public string? Description { get; set; } = string.Empty;
        public TaskPriority Priority { get; set; } = TaskPriority.High;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsActive { get; set; } = false;

        public List<TaskUser> taskUsers { get; set; } = new List<TaskUser>();
        public List<Issue> issues { get; set; } = new List<Issue>();
        public TaskProject()
        {
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
    }

}
