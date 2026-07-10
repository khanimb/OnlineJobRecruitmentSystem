using FluentValidation;
using OnlineJobRecruitmentSystem.Application.DTOs.UserDtos;

namespace OnlineJobRecruitmentSystem.Application.Validations.UserDtoValidation
{
    public class ResetPasswordDtoValidation : AbstractValidator<ResetPasswordDto>
    {
        public ResetPasswordDtoValidation()
        {
            RuleFor(x => x.Token)
                .NotEmpty().WithMessage("Token cannot be empty.");

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("New password cannot be empty.")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters.");

            RuleFor(x => x.ConfirmPassword)
                .Equal(x => x.NewPassword).WithMessage("Passwords do not match.");
        }
    }
}