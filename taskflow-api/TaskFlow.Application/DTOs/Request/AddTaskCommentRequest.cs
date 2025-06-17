using System.ComponentModel.DataAnnotations;
using taskflow_api.TaskFlow.Domain.Entities;

namespace taskflow_api.TaskFlow.Application.DTOs.Request
{
    public class AddTaskCommentRequest
    {
        public Guid TaskId { get; set; }
        public Guid Commenter { get; set; }
        public string? Content { get; set; } = string.Empty;
    }
}
