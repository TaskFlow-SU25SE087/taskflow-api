using System.ComponentModel.DataAnnotations;

namespace taskflow_api.TaskFlow.Domain.Entities
{
    public class TaskLabels
    {
        [Key]
        public Guid Id { get; set; }
        public Guid TaskId { get; set; }
        public TaskProject Task { get; set; } = null!;
        public Guid LabelId { get; set; }
        public Labels Label { get; set; } = null!;
    }
}
