using taskflow_api.TaskFlow.Domain.Common.Enums;

namespace taskflow_api.TaskFlow.Application.DTOs.Response
{
    public class TaskProjectResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public TaskPriority Priority { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string Status { get; set; } = null!;
        public List<TaskAssigneeResponse> TaskAssignees { get; set; } = new List<TaskAssigneeResponse>();
        public List<TaskTagResponse> Tags { get; set; } = new List<TaskTagResponse>();

        }
        public class TaskAssigneeResponse
        {
            public string? Executor { get; set; }
            public string? Avatar { get; set; }
            public ProjectRole Role { get; set; }
        }

        public class TaskTagResponse
        {
            public string? name { get; set; }
            public string? description { get; set; }
        }
}
