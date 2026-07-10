using FluentValidation;
using OnlineJobRecruitmentSystem.Application.DTOs.UserDtos;

namespace OnlineJobRecruitmentSystem.Application.Validations.UserDtoValidation
{
    public class ForgotPasswordDtoValidation : AbstractValidator<ForgotPasswordDto>
    {
        public ForgotPasswordDtoValidation()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email cannot be empty.")
                .EmailAddress().WithMessage("Invalid email format.");
        }
    }
}