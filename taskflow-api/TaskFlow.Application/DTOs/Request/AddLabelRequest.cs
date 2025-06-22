using System.ComponentModel.DataAnnotations;

namespace taskflow_api.TaskFlow.Application.DTOs.Request
{
    public class AddTagRequest
    {
        [Required(ErrorMessage ="Name cannot empty")]
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        [Required(ErrorMessage = "Color cannot empty")]
        public string Color { get; set; } = null!; 
    }
}
