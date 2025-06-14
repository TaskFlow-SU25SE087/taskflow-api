using System.ComponentModel.DataAnnotations;

namespace taskflow_api.TaskFlow.Domain.Entities
{
    public class TaskTag
    {
        [Key]
        public Guid Id { get; set; } = new Guid();
        public Guid TaskId { get; set; }
        public TaskProject Task { get; set; } = null!;
        public Guid TagId { get; set; }
        public Tag Tag { get; set; } = null!;
    }
}
