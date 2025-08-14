using System.ComponentModel.DataAnnotations;
using taskflow_api.TaskFlow.Domain.Common.Enums;

namespace taskflow_api.TaskFlow.Application.DTOs.Request
{
    public class UpdateTaskRequest
    {
        [Required(ErrorMessage = "Title cannot be empty")]
        [MaxLength(100, ErrorMessage = "Title cannot exceed 100 characters")]
        public string Title { get; set; } = string.Empty;
        [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; } = string.Empty;
        [Required(ErrorMessage = "Board ID cannot be empty")]
        public TaskPriority Priority { get; set; }
        public DateTime? Deadline { get; set; }
    }
}
