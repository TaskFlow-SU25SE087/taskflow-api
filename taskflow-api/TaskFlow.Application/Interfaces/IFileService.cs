using CloudinaryDotNet.Actions;

namespace taskflow_api.TaskFlow.Application.Interfaces
{
    public interface IFileService
    {
        Task<string> UploadPictureAsync(IFormFile file);
        Task<string> UploadAutoAsync(IFormFile file);
    }
}
