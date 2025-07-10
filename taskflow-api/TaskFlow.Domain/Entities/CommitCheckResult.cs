using System.ComponentModel.DataAnnotations;

namespace taskflow_api.TaskFlow.Domain.Entities
{
    public class CommitCheckResult
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid CommitRecordId { get; set; }
        public CommitRecord CommitRecord { get; set; } = null!;

        [Required]
        public string ResultType { get; set; } = string.Empty;
        public string? Value { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
