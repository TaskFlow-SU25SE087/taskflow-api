using System.ComponentModel.DataAnnotations;
using taskflow_api.TaskFlow.Domain.Common.Enums;

namespace taskflow_api.TaskFlow.Domain.Entities
{
    public class LogProject
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required]
        public Guid ProjectMemberId { get; set; }
        public ProjectMember ProjectMember { get; set; } = null!;

        [Required]
        public Guid ProjectId { get; set; }
        public Project Project { get; set; } = null!;
        public Guid? TaskProjectID { get; set; }
        public TaskProject? TaskProject { get; set; }
        public Guid? SprintId { get; set; }
        public Sprint? Sprint { get; set; }
        public Guid? BoardId { get; set; }
        public Board? Board { get; set; }

        public TypeLog ActionType { get; set; }
        public ChangedField? FieldChanged { get; set; }
        public string? OldValue { get; set; } = string.Empty;
        public string? NewValue { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}
