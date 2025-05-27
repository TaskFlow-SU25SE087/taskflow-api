using System.ComponentModel.DataAnnotations;

namespace taskflow_api.TaskFlow.Domain.Entities
{
    public class Board
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public Guid ProjectId { get; set; }
        public Project Project { get; set; } = null!;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;
        public int Order { get; set; } = 0;
        public bool IsActive { get; set; } = false;
        public List<TaskProject> TaskProject { get; set; } = new List<TaskProject>();
    }
}
