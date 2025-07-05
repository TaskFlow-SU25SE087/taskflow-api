using System.ComponentModel.DataAnnotations;

namespace taskflow_api.TaskFlow.Domain.Entities
{
    public class CommitRecord
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid ProjectPartId { get; set; }
        public ProjectPart ProjectPart { get; set; } = null!;

        [Required]
        public string CommitId { get; set; } = string.Empty;
        [Required]

        public string Pusher { get; set; } = string.Empty;
        public DateTime PushedAt { get; set; } = DateTime.UtcNow;

        public string? CommitMessage { get; set; }
        public string? ResultSummary { get; set; }
        public string? Notes { get; set; }
    }
}
