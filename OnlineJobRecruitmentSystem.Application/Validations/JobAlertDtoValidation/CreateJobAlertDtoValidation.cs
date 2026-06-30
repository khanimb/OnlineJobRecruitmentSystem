using FluentValidation;
using OnlineJobRecruitmentSystem.Application.DTOs.JobAlertDtos;

namespace OnlineJobRecruitmentSystem.Application.Validations.JobAlertDtoValidation
{
    public class CreateJobAlertDtoValidation : AbstractValidator<CreateJobAlertDto>
    {
        public CreateJobAlertDtoValidation()
        {
            RuleFor(x => x.Keyword)
                .MaximumLength(100).WithMessage("Keyword cannot exceed 100 characters.");

            RuleFor(x => x.Location)
                .MaximumLength(100).WithMessage("Location cannot exceed 100 characters.");

            RuleFor(x => x.Frequency)
                .NotEmpty().WithMessage("Frequency is required.")
                .Must(f => f == "daily" || f == "weekly")
                .WithMessage("Frequency must be 'daily' or 'weekly'.");
        }
    }
}