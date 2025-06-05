using System.ComponentModel.DataAnnotations;

namespace taskflow_api.TaskFlow.Domain.Entities
{
    public class UserReports
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required]
        public Guid UserReportId { get; set; }
        [Required]
        public Guid UserBeenReportId { get; set; }
        [Required]
        public string ReportReason { get; set; } = null!;
        public DateTime ReportDate { get; set; } = DateTime.UtcNow;
    }
}
