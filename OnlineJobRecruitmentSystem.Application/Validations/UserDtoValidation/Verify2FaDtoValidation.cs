using FluentValidation;
using OnlineJobRecruitmentSystem.Application.DTOs.UserDtos;

namespace OnlineJobRecruitmentSystem.Application.Validations.UserDtoValidation
{
    public class Verify2FaDtoValidation : AbstractValidator<Verify2FaDto>
    {
        public Verify2FaDtoValidation()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email cannot be empty.")
                .EmailAddress().WithMessage("Invalid email format.");

            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Verification code cannot be empty.")
                .Length(6).WithMessage("Verification code must be 6 digits.");
        }
    }
}