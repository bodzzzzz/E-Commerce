using Microsoft.AspNetCore.Http;

namespace E_Commerce.Helpers
{
    public static class FileHelper
    {
        public static async Task<string> SaveImageAsync(IFormFile file, string webRootPath)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("No file uploaded");

            // Validate file type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
                throw new ArgumentException("Invalid file type. Only jpg, jpeg, png, and gif are allowed.");

            // Create unique filename
            var fileName = $"{Guid.NewGuid()}{extension}";
            
            // Create URL-friendly path (using forward slashes)
            var urlPath = $"/images/products/{fileName}";
            
            // Create filesystem path
            var absolutePath = Path.Combine(webRootPath, "images", "products", fileName);

            // Ensure directory exists
            var directory = Path.GetDirectoryName(absolutePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Save file
            using (var stream = new FileStream(absolutePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Return URL-friendly path
            return urlPath;
        }

        public static void DeleteImage(string imagePath, string webRootPath)
        {
            if (string.IsNullOrEmpty(imagePath)) return;

            // Convert URL path to filesystem path
            var relativePath = imagePath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString());
            var fullPath = Path.Combine(webRootPath, relativePath);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
    }
}