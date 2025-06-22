using System.ComponentModel.DataAnnotations;

namespace taskflow_api.TaskFlow.Application.DTOs.Request
{
    public class UpdateTagRequest
    {
        [Required(ErrorMessage = "Name cannot empty")]
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string Color { get; set; } = null!;
    }
}
