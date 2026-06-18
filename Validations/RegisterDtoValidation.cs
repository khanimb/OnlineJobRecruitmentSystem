using FluentValidation;
using OnlineJobRecruitmentSystem.DTOs;

namespace OnlineJobRecruitmentSystem.Validations
{
    public class RegisterDtoValidation : AbstractValidator<RegisterDto>
    {
        public RegisterDtoValidation()
        {
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("Username cannot be empty.")
                .MinimumLength(3).WithMessage("Username must be at least 3 characters.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email cannot be empty.")
                .EmailAddress().WithMessage("Invalid email format.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password cannot be empty.")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters.");

            RuleFor(x => x.Role)
                .NotEmpty().WithMessage("Role cannot be empty.")
                .Must(r => r == "Employer" || r == "JobSeeker")
                .WithMessage("Role must be 'Employer' or 'JobSeeker'.");
        }
    }
}
