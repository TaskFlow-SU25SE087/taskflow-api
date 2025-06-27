using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using taskflow_api.TaskFlow.Domain.Common.Enums;
using taskflow_api.TaskFlow.Domain.Entities;

namespace taskflow_api.TaskFlow.Application.DTOs.Request
{
    public class CreateTaskIssueRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public TaskPriority Priority { get; set; }
        public string? Explanation { get; set; } = string.Empty;
        public string? Example { get; set; } = string.Empty;
        public TypeIssue Type { get; set; }
        public List<IFormFile> Files { get; set; } = new();
    }
}
