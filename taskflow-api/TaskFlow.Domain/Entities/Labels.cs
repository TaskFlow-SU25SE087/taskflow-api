using System.ComponentModel.DataAnnotations;

namespace taskflow_api.TaskFlow.Domain.Entities
{
    public class Labels
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public Guid ProjectId { get; set; }
        public Project Project { get; set; } = null!;
        [Required]
        [MaxLength(50)]
        public string Name { get; set; }
        [MaxLength(100)]
        public string? Description { get; set; }
        public List<TaskLabels> TaskLabels { get; set; } = new List<TaskLabels>();
    }
}
