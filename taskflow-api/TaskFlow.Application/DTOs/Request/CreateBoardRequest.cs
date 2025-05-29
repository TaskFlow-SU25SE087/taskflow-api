using System.ComponentModel.DataAnnotations;
using taskflow_api.TaskFlow.Domain.Entities;

namespace taskflow_api.TaskFlow.Application.DTOs.Request
{
    public class CreateBoardRequest
    {
        public Guid ProjectId { get; set; }
        [Required(ErrorMessage = "Board name is required.")]
        public string Name { get; set; } = string.Empty;
        [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string? Description { get; set; } = string.Empty;
        public bool IsActive { get; set; } = false;
    }
}
