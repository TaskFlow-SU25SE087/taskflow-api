using System.ComponentModel.DataAnnotations;
using taskflow_api.TaskFlow.Domain.Common.Enums;

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
        public StatusCommit Status { get; set; } = StatusCommit.Checking;

        public string? CommitUrl { get; set; }
        public string? CommitMessage { get; set; }
        public string? ResultSummary { get; set; }
        public DateTime? ExpectedFinishAt { get; set; }

        public List<CommitCheckResult> CheckResults { get; set; } = new List<CommitCheckResult>();
    }
}
