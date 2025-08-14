using taskflow_api.TaskFlow.Domain.Common.Enums;

namespace taskflow_api.TaskFlow.Application.DTOs.Response
{
    public class ListTaskProjectNotSprint
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;
        public TaskPriority Priority { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? Deadline { get; set; }
        public string? AttachmentUrl { get; set; } = string.Empty;
        public Guid? BoardId { get; set; }
        public string BoardName { get; set; } = string.Empty;
    }
}
