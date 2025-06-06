using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using taskflow_api.TaskFlow.Application.Interfaces;

namespace taskflow_api.TaskFlow.Application.Services
{
    public class PhotoService : IPhotoService
    {
        private readonly Cloudinary _cloudinary;

        public PhotoService(Cloudinary cloudinary)
        {
            _cloudinary = cloudinary;
        }

        public async Task<string> UploadPhotoAsync(IFormFile file)
        {
            if (file.Length > 0)
            {
                await using var stream = file.OpenReadStream();
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, stream),

                    Transformation = new Transformation()
                        .Width(500)
                        .Height(500)
                        .Crop("pad")           
                        .Background("white")  
                        .FetchFormat("jpg"),
                    Folder = "avatar"
                };

                var result = await _cloudinary.UploadAsync(uploadParams);
                return result.SecureUrl.ToString();
            }

            return null;
        }
    }
}
