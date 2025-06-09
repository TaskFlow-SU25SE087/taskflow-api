using System.ComponentModel.DataAnnotations;

namespace taskflow_api.TaskFlow.Domain.Entities
{
    public class Tag
    {
        [Key]
        public Guid Id { get; set; } = new Guid();
        [Required]
        public Guid ProjectId { get; set; }
        public Project Project { get; set; } = null!;
        [Required]
        [MaxLength(50)]
        public string Name { get; set; }
        [MaxLength(100)]
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public List<TaskTag> TaskTags { get; set; } = new List<TaskTag>();
    }
}
