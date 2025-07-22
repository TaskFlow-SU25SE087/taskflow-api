using System.ComponentModel.DataAnnotations;
using taskflow_api.TaskFlow.Domain.Common.Enums;
using taskflow_api.TaskFlow.Domain.Entities;

namespace taskflow_api.TaskFlow.Application.DTOs.Request
{
    public class CreateSprintRequest
    {
        [Required(ErrorMessage = "name cannot be blank")]
        [StringLength(100, ErrorMessage = "Sprint name cannot exceed 100 characters.")]
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
