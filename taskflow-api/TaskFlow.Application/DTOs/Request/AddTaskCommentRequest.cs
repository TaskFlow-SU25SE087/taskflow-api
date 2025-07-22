using System.ComponentModel.DataAnnotations;
using taskflow_api.TaskFlow.Application.DTOs.Common.Attributes;
using taskflow_api.TaskFlow.Domain.Entities;

namespace taskflow_api.TaskFlow.Application.DTOs.Request
{
    public class AddTaskCommentRequest
    {
        public string? Content { get; set; } = string.Empty;
        [MaxFileCount(3)]
        public List<IFormFile>? Files { get; set; } = new();
    }
}
