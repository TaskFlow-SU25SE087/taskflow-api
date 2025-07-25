﻿using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using taskflow_api.TaskFlow.Application.Interfaces;

namespace taskflow_api.TaskFlow.Application.Services
{
    public class FileService : IFileService
    {
        private readonly Cloudinary _cloudinary;

        public FileService(Cloudinary cloudinary)
        {
            _cloudinary = cloudinary;
        }

        public async Task<string> UploadPictureAsync(IFormFile file)
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

        public async Task<string> UploadAutoAsync(IFormFile file)
        {
            await using var stream = file.OpenReadStream();
            var extension = Path.GetExtension(file.FileName).ToLower();

            if (extension == ".jpg" || extension == ".png" || extension == ".jpeg")
            {
                var imageParams = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    Folder = "project/image"
                };
                return await UploadAsync(imageParams);
            }
            else if (extension == ".mp4")
            {
                var videoParams = new VideoUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    Folder = "project/videos"
                };
                return await UploadAsync(videoParams);
            }
            else
            {
                var rawParams = new RawUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    Folder = "project/files"
                };
                return await UploadAsync(rawParams);
            }
        }

        private async Task<string> UploadAsync(ImageUploadParams uploadParams)
        {
            var result = await _cloudinary.UploadAsync(uploadParams);
            return result.SecureUrl.ToString();
        }

        private async Task<string> UploadAsync(VideoUploadParams uploadParams)
        {
            var result = await _cloudinary.UploadAsync(uploadParams);
            return result.SecureUrl.ToString();
        }
        private async Task<string> UploadAsync(RawUploadParams uploadParams)
        {
            var result = await _cloudinary.UploadAsync(uploadParams);
            return result.SecureUrl.ToString();
        }
    }
}
