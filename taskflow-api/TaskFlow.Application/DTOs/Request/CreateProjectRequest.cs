using System.ComponentModel.DataAnnotations;

namespace taskflow_api.TaskFlow.Application.DTOs.Request
{
    public class CreateProjectRequest
    {
        [Required(ErrorMessage = "Please enter project name")]
        public string title { get; set; } = null!;
    }
}
