using System.ComponentModel.DataAnnotations;
using taskflow_api.TaskFlow.Domain.Common.Enums;
using taskflow_api.TaskFlow.Domain.Entities;

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
        public List<CommnetResponse> commnets { get; set; } = new List<CommnetResponse>();


        }
        public class TaskAssigneeResponse
        {
            public string? Executor { get; set; }
            public string? Avatar { get; set; }
            public ProjectRole Role { get; set; }
        }

        public class TaskTagResponse
        {
            public string? Name { get; set; }
            public string? Description { get; set; }
            public string Color { get; set; } = null!;
        }

        public class CommnetResponse
        {
            public string Commenter { get; set; } = null!;
            public string? Content { get; set; } = string.Empty;
            public string Avatar { get; set; } = null!;
            public DateTime LastUpdate { get; set; }
        }
}
