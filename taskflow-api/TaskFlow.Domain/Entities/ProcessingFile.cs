using System.ComponentModel.DataAnnotations;
using taskflow_api.TaskFlow.Domain.Common.Enums;

namespace taskflow_api.TaskFlow.Domain.Entities
{
    public class ProcessingFile
    {
        [Key]
        public Guid Id { get; set; }
        public string FileName { get; set; } = null!;
        public string Note { get; set; } = string.Empty;
        [Required]
        public string UrlFile { get; set; } = null!;
        public StatusFile statusFile { get; set; } = StatusFile.Processing;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; } = null;
    }
}
