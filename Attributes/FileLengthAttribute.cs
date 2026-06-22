using System.ComponentModel.DataAnnotations;

namespace OnlineJobRecruitmentSystem.Attributes
{
    public class FileLengthAttribute : ValidationAttribute
    {
        private readonly long _maxBytes;

        public FileLengthAttribute(long maxBytes)
        {
            _maxBytes = maxBytes;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is IFormFile file && file.Length > _maxBytes)
                return new ValidationResult($"File size must not exceed {_maxBytes / (1024 * 1024)} MB.");

            return ValidationResult.Success;
        }
    }
}
