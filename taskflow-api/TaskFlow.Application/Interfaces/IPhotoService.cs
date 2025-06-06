using CloudinaryDotNet.Actions;

namespace taskflow_api.TaskFlow.Application.Interfaces
{
    public interface IPhotoService
    {
        Task<string> UploadPhotoAsync(IFormFile file);
    }
}
