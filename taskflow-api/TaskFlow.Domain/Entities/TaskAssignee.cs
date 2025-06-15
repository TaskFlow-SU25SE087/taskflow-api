using System.ComponentModel.DataAnnotations;

namespace taskflow_api.TaskFlow.Domain.Entities
{
    public class TaskAssignee
    {
        [Key]
        public Guid Id { get; set; }
        public Guid? Assigner { get; set; }
        public Guid TaskId { get; set; }
        public TaskProject Task { get; set; } = null!;
        public Guid Implementer { get; set; }
        public ProjectMember ProjectMember { get; set; } = null!;
        public Guid? IssueID { get; set; }
        public Issue? Issue { get; set; } = null!;

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsActive { get; set; } = false;


        public TaskAssignee()
        {
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

    }
}
