using System.ComponentModel.DataAnnotations;
using taskflow_api.TaskFlow.Domain.Common.Enums;

namespace taskflow_api.TaskFlow.Application.DTOs.Request
{
    public class CreateProjectRequest
    {
        [Required(ErrorMessage = "Please enter project name")]
        public string Title { get; set; } = null!;
        [Required(ErrorMessage = "Please enter project description")]
        public string Description { get; set; } = null!;
    }
}
