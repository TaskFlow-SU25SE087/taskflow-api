using System.ComponentModel.DataAnnotations;
using taskflow_api.TaskFlow.Domain.Common.Enums;

namespace taskflow_api.TaskFlow.Domain.Entities
{
    public class Board
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required]
        public Guid ProjectId { get; set; }
        public Project Project { get; set; } = null!;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;
        public int Order { get; set; } = 0;
        public bool IsActive { get; set; } = false;
        public BoardType Type { get; set; } = BoardType.Custom;
        public List<TaskProject> TaskProject { get; set; } = new List<TaskProject>();
    }
}
