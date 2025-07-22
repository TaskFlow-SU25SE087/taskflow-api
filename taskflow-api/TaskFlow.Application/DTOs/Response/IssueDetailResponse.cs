using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using taskflow_api.TaskFlow.Domain.Common.Enums;
using taskflow_api.TaskFlow.Domain.Entities;

namespace taskflow_api.TaskFlow.Application.DTOs.Response
{
    public class IssueDetailResponse
    {
        public Guid Id { get; set; }

        public Guid? TaskProjectID { get; set; }
        public string TitleTask { get; set; }
        public TaskPriority? PriorityTask { get; set; }

        public Guid CreatedBy { get; set; }
        public string NameCreate { get; set; } = string.Empty;
        public string AvatarCreate { get; set; } = string.Empty;
        public ProjectRole RoleCreate { get; set; }

        public string Title { get; set; }
        public string Description { get; set; } 
        public string? Explanation { get; set; } 
        public string? Example { get; set; } 
        public TaskPriority Priority { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public TypeIssue Type { get; set; }
        public IssueStatus Status { get; set; }
        public List<string> IssueAttachmentUrls { get; set; } = new List<string>();
        public List<TaskAssigneeResponseInIssue> TaskAssignees { get; set; }
    }

    public class TaskAssigneeResponseInIssue
    {
        public Guid ProjectMemberId { get; set; }
        public string? Executor { get; set; }
        public string? Avatar { get; set; }
        public ProjectRole Role { get; set; }
    }
}
