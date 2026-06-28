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
    }
}