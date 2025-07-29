using taskflow_api.TaskFlow.Domain.Common.Enums;

namespace taskflow_api.TaskFlow.Application.DTOs.Response
{
    public class UnfinishedTaskResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;
        public TaskPriority Priority { get; set; } = TaskPriority.High;
        public string? Reason { get; set; } = string.Empty;
        public int ItemVersion { get; set; } = 0;

        public Guid SprintMeetingId { get; set; }
        public string SprintName { get; set; } = string.Empty;
        public DateTime UpdateDeadline { get; set; }
    }
}
