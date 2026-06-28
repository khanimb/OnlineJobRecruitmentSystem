using FluentValidation;
using OnlineJobRecruitmentSystem.Application.DTOs.UserDtos;

namespace OnlineJobRecruitmentSystem.Application.Validations.UserDtoValidation
{
    public class LoginDtoValidation : AbstractValidator<LoginDto>
    {
        public LoginDtoValidation()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email cannot be empty.")
                .EmailAddress().WithMessage("Invalid email format.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password cannot be empty.")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters.");
        }
    }
}
