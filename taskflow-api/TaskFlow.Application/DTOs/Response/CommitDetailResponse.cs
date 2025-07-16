using System.ComponentModel.DataAnnotations;
using taskflow_api.TaskFlow.Domain.Entities;

namespace taskflow_api.TaskFlow.Application.DTOs.Response
{
    public class CommitDetailResponse
    {
        public string Rule { get; set; } = string.Empty;
        [Required]
        public string Severity { get; set; } = string.Empty;

        [Required]
        public string Message { get; set; } = string.Empty;

        [Required]
        public string FilePath { get; set; } = string.Empty;
        public int? Line { get; set; }
        public string? LineContent { get; set; } = string.Empty;
    }
}
