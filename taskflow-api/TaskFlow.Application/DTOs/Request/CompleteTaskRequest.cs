using taskflow_api.TaskFlow.Application.DTOs.Common.Attributes;

namespace taskflow_api.TaskFlow.Application.DTOs.Request
{
    public class CompleteTaskRequest
    {
        [MaxFileCount(2)]
        public List<IFormFile> Files { get; set; } = new();
    }
}
