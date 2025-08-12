using taskflow_api.TaskFlow.Domain.Common.Enums;

namespace taskflow_api.TaskFlow.Application.DTOs.Common
{
    public class SprintTaskWithBoardInfo
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;
        public TaskPriority Priority { get; set; }
        public DateTime Deadline { get; set; }
        public Guid? BoardId { get; set; }
        public string BoardName { get; set; } = string.Empty;
        public BoardType BoardType { get; set; }
        public List<string> Assignees { get; set; } = new List<string>();
    }
} 