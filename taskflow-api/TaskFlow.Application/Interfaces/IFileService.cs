using CloudinaryDotNet.Actions;

namespace taskflow_api.TaskFlow.Application.Interfaces
{
    public interface IFileService
    {
        Task<string> UploadFileAsync(IFormFile file);
    }
}
