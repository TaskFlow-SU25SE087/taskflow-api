using System.ComponentModel.DataAnnotations;
using taskflow_api.TaskFlow.Domain.Common.Enums;

namespace taskflow_api.TaskFlow.Application.DTOs.Request
{
    public class UpdateSprintRequest
    {
        public Guid SprintId { get; set; }
        public Guid ProjectId { get; set; }
        [Required(ErrorMessage = "name cannot be blank")]
        [StringLength(100, ErrorMessage = "Sprint name cannot exceed 100 characters.")]
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public SprintStatus Status { get; set; } 
    }
}
