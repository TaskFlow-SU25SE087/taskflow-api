using System.ComponentModel.DataAnnotations;

namespace taskflow_api.TaskFlow.Domain.Entities
{
    public class SprintMeetingLog
    {
        [Key]
        public Guid Id { get; set; }

        public Guid SprintId { get; set; }
        public Sprint Sprint { get; set; } = null!;
        //json
        public string CompletedTasksJson { get; set; } = string.Empty;
        public string UnfinishedTasksJson { get; set; } = string.Empty;

        public string NextPlan { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; } = null;

    }
}
