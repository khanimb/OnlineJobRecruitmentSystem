using Microsoft.AspNetCore.Http;

namespace OnlineJobRecruitmentSystem.Infrastructure.Extensions
{
    public static class FileExtensions
    {
        public static bool IsValidSize(this IFormFile file, long maxBytes)
            => file.Length <= maxBytes;

        public static bool IsValidType(this IFormFile file, params string[] allowedExtensions)
        {
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            return allowedExtensions.Contains(ext);
        }

    public static bool HasValidSignature(this IFormFile file, params string[] allowedExtensions)
        {
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(ext)) return false;

            using var stream = file.OpenReadStream();
            var buffer = new byte[8];
            var bytesRead = stream.Read(buffer, 0, buffer.Length);
            stream.Position = 0;

            return ext switch
            {
                ".pdf" => bytesRead >= 4 && buffer[0] == 0x25 && buffer[1] == 0x50 && buffer[2] == 0x44 && buffer[3] == 0x46,
                ".jpg" or ".jpeg" => bytesRead >= 3 && buffer[0] == 0xFF && buffer[1] == 0xD8 && buffer[2] == 0xFF,
                ".png" => bytesRead >= 8 && buffer[0] == 0x89 && buffer[1] == 0x50 && buffer[2] == 0x4E && buffer[3] == 0x47,
                ".doc" => bytesRead >= 4 && buffer[0] == 0xD0 && buffer[1] == 0xCF && buffer[2] == 0x11 && buffer[3] == 0xE0,
                ".docx" => bytesRead >= 4 && buffer[0] == 0x50 && buffer[1] == 0x4B && buffer[2] == 0x03 && buffer[3] == 0x04,
                _ => true
            };
        }
    }
}