using FluentValidation;
using OnlineJobRecruitmentSystem.Application.DTOs.JobDtos;

namespace OnlineJobRecruitmentSystem.Application.Validations.JobDtoValidation
{
    public class UpdateJobDtoValidation : AbstractValidator<UpdateJobDto>
    {
        public UpdateJobDtoValidation()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title cannot be empty.")
                .MaximumLength(100).WithMessage("Title must be at most 100 characters.");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description cannot be empty.");

            RuleFor(x => x.Location)
                .NotEmpty().WithMessage("Location cannot be empty.");

            RuleFor(x => x.JobType)
                .NotEmpty().WithMessage("Job type cannot be empty.")
                .Must(t => t == "FullTime" || t == "PartTime" || t == "Remote")
                .WithMessage("Job type must be 'FullTime', 'PartTime' or 'Remote'.");

            RuleFor(x => x.SalaryMin)
                .GreaterThan(0).WithMessage("Minimum salary must be greater than 0.");

            RuleFor(x => x.SalaryMax)
                .GreaterThan(x => x.SalaryMin).WithMessage("Maximum salary must be greater than minimum salary.");

            RuleFor(x => x.Deadline)
                .GreaterThan(DateTime.UtcNow).WithMessage("Deadline must be a future date.");
        }
    }
}
