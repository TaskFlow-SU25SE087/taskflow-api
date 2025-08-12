using taskflow_api.TaskFlow.Domain.Common.Enums;

namespace taskflow_api.TaskFlow.Application.DTOs.Common
{
    public class UnfinishedTaskDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;
        public TaskPriority Priority { get; set; } = TaskPriority.High;
        public string? Reason { get; set; } = string.Empty;
        public int ItemVersion { get; set; } = 0;
    }
}
