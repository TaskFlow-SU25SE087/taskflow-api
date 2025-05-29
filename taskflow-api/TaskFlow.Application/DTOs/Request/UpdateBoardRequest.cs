using System.ComponentModel.DataAnnotations;

namespace taskflow_api.TaskFlow.Application.DTOs.Request
{
    public class UpdateBoardRequest
    {
        public Guid BoardId { get; set; }
        public Guid ProjectId { get; set; }
        [Required(ErrorMessage = "Board name is required.")]
        public string Name { get; set; } = string.Empty;
        [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string? Description { get; set; } = string.Empty;
        public int Order { get; set; }
        public bool IsActive { get; set; } 
    }
}
