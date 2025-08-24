using System.ComponentModel.DataAnnotations;
using taskflow_api.TaskFlow.Domain.Common.Enums;
using taskflow_api.TaskFlow.Domain.Entities;

namespace taskflow_api.TaskFlow.Application.DTOs.Request
{
    public class AddTaskRequest
    {
        [Required(ErrorMessage = "Title cannot be empty")]
        [MaxLength(100, ErrorMessage = "Title cannot exceed 100 characters")]
        public string Title { get; set; } = string.Empty;
        [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; } = string.Empty;
        [Required(ErrorMessage = "Priority cannot null")]
        public TaskPriority Priority { get; set; } = TaskPriority.High;
        public DateTime? Deadline { get; set; }
        public IFormFile? File { get; set; }
        
        // Effort points for the task
        [Range(0, int.MaxValue, ErrorMessage = "Effort points must be a positive number")]
        public int? EffortPoints { get; set; }
    }
}
