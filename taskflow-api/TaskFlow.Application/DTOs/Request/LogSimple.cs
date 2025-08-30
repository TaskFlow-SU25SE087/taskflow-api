using taskflow_api.TaskFlow.Domain.Common.Enums;

namespace taskflow_api.TaskFlow.Application.DTOs.Request
{
    public class LogSimple
    {
        public Guid ProjectMemberId { get; set; }
        public Guid ProjectId { get; set; }
        public TypeLog ActionType { get; set; }
        public string? Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
