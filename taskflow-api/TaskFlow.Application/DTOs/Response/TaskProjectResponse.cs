using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
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
        public DateTime Deadline { get; set; }
        public string SprintName { get; set; } = string.Empty;
        public string? AttachmentUrl { get; set; } = string.Empty;
        [JsonIgnore]
        public string? CompletionAttachmentUrl { get; set; } = string.Empty;
        public List<string> CompletionAttachmentUrls
        {
            get => string.IsNullOrEmpty(CompletionAttachmentUrl) ? new List<string>() : CompletionAttachmentUrl.Split('|').ToList();
            set => CompletionAttachmentUrl = string.Join('|', value);
        }
        public string Status { get; set; } = null!;
        public List<TaskAssigneeResponse> TaskAssignees { get; set; } = new List<TaskAssigneeResponse>();
        public List<TaskTagResponse> Tags { get; set; } = new List<TaskTagResponse>();
        public List<CommnetResponse> Commnets { get; set; } = new List<CommnetResponse>();
        public List<IssueTaskResponse> Issues { get; set; } = new List<IssueTaskResponse>();



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
            public List<string> AttachmentUrls { get; set; } = new List<string>();
            public DateTime LastUpdate { get; set; }
        }
        public class IssueTaskResponse
        {
            public Guid Id { get; set; }
            public string? Title { get; set; }
            public string? Description { get; set; }
            public string Explanation { get; set; } = null!;
            public string? Example { get; set; }
            public TaskPriority Priority { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime UpdatedAt { get; set; }
            public TypeIssue Type { get; set; }
            public IssueStatus Status { get; set; } = IssueStatus.Open;
            public List<string> IssueAttachmentUrls { get; set; } = new List<string>();

    }

}
