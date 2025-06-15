using System.ComponentModel.DataAnnotations;

namespace taskflow_api.TaskFlow.Application.DTOs.Request
{
    public class ImportUserFileRequest
    {
        [Required(ErrorMessage = "No FIle")]
        public IFormFile File { get; set; } = default!;
    }
}
