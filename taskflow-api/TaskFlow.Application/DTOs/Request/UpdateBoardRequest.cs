using System.ComponentModel.DataAnnotations;
using taskflow_api.TaskFlow.Domain.Common.Enums;

namespace taskflow_api.TaskFlow.Application.DTOs.Request
{
    public class UpdateBoardRequest
    {
        [Required(ErrorMessage = "Board name is required.")]
        public string Name { get; set; } = string.Empty;
        [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string? Description { get; set; } = string.Empty;
        public BoardType? Type { get; set; }
    }
}
