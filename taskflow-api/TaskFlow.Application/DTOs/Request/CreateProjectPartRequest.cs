using System.ComponentModel.DataAnnotations;
using taskflow_api.TaskFlow.Domain.Common.Enums;

namespace taskflow_api.TaskFlow.Application.DTOs.Request
{
    public class CreateProjectPartRequest
    {
        [Required(ErrorMessage = "Name is required.")]
        public string Name { get; set; } = string.Empty;
        [Required(ErrorMessage = "Programming language is required.")]
        public ProgrammingLanguage ProgrammingLanguage { get; set; } = ProgrammingLanguage.None;
        [Required(ErrorMessage = "Framework is required.")]
        public Framework Framework { get; set; } = Framework.None;
    }
}
