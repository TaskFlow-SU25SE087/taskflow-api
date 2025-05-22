using System.IO;
using taskflow_api.Enums;
using taskflow_api.Exceptions;

namespace taskflow_api.Helpers
{
    public class ImageHelper
    {
        public static async Task<string> UploadImage(IFormFile file,
            string wwwRootPath, string folderPath, string fileName)
        {
            if (file.Length > 0)
            {
                var extension = Path.GetExtension(file.FileName);
                var saveFolderPath = Path.Combine(wwwRootPath, folderPath);
                var fullPath = Path.Combine(saveFolderPath, fileName + extension);

                // Check if the file already exists
                if (!Directory.Exists(saveFolderPath))
                {
                    Directory.CreateDirectory(saveFolderPath);
                }

                //save the file
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var relativePath = Path.Combine(folderPath, fileName + extension).Replace("\\", "/");
                return relativePath;
            }
            return string.Empty;
        }

        public static void DeleteImage(string relativePath, string webRootPath)
        {
            var fullPath = Path.Combine(webRootPath, relativePath);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
    }
}