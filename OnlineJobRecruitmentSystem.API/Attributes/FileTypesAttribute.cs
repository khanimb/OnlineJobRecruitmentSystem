using System.ComponentModel.DataAnnotations;

namespace OnlineJobRecruitmentSystem.API.Attributes
{
    public class FileTypesAttribute : ValidationAttribute
    {
        private readonly string[] _allowedTypes;

        public FileTypesAttribute(params string[] allowedTypes)
        {
            _allowedTypes = allowedTypes;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is IFormFile file)
            {
                var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!_allowedTypes.Contains(ext))
                    return new ValidationResult($"Allowed file types: {string.Join(", ", _allowedTypes)}");
            }

            return ValidationResult.Success;
        }
    }
}
