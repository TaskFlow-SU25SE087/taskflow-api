using System.ComponentModel.DataAnnotations;

namespace taskflow_api.TaskFlow.Domain.Entities
{
    public class CommitCheckResult
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid CommitRecordId { get; set; }
        public CommitRecord CommitRecord { get; set; } = null!;

        public bool Result { get; set; } = false;
        public string OutputLog { get; set; } = string.Empty;
        public string ErrorLog { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
