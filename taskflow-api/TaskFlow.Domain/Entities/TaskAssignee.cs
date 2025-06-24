using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using taskflow_api.TaskFlow.Domain.Common.Enums;

namespace taskflow_api.TaskFlow.Domain.Entities
{
    public class TaskAssignee
    {
        [Key]
        public Guid Id { get; set; } = new Guid();
        public Guid RefId { get; set; }
        public Guid? AssignerId { get; set; }
        public Guid ImplementerId { get; set; }
        [ForeignKey("ImplementerId")]
        public ProjectMember ProjectMember { get; set; } = null!;
        public RefType Type { get; set; } = RefType.None;
        public string? CancellationNote { get; set; }
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
