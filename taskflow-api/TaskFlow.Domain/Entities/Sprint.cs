using System.ComponentModel.DataAnnotations;
using taskflow_api.TaskFlow.Domain.Common.Enums;

namespace taskflow_api.TaskFlow.Domain.Entities
{
    public class Sprint
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public Guid ProjectId { get; set; }
        public Project Project { get; set; } = null!;

        [Required]
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public SprintStatus Status { get; set; } = SprintStatus.NotStarted;
        public List<TaskProject> TaskProject { get; set; } = new List<TaskProject>();
    }
}
