using FluentValidation;
using OnlineJobRecruitmentSystem.Application.DTOs.ApplicationDtos;

namespace OnlineJobRecruitmentSystem.Application.Validations.ApplicationDtoValidation
{
    public class CreateApplicationDtoValidation : AbstractValidator<CreateApplicationDto>
    {
        public CreateApplicationDtoValidation()
        {
            RuleFor(x => x.CoverLetter)
                .NotEmpty().WithMessage("Cover letter is required.")
                .MaximumLength(2000).WithMessage("Cover letter cannot exceed 2000 characters.");
        }
    }
}