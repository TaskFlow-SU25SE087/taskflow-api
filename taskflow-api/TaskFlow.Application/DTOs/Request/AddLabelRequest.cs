using System.ComponentModel.DataAnnotations;

namespace taskflow_api.TaskFlow.Application.DTOs.Request
{
    public class AddLabelRequest
    {
        [Required(ErrorMessage ="Name cannot empty")]
        public string Name { get; set; } = null!;
        public Guid ProjectId { get; set; }
        public string? Description { get; set; }
    }
}
