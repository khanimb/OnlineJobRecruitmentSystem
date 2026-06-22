using FluentValidation;
using OnlineJobRecruitmentSystem.DTOs.JobSeekerDtos;

namespace OnlineJobRecruitmentSystem.Validations.JobSeekerDtoValidation
{
    public class CreateJobSeekerDtoValidation : AbstractValidator<CreateJobSeekerDto>
    {
        public CreateJobSeekerDtoValidation()
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Full name is required.")
                .MaximumLength(100).WithMessage("Full name cannot exceed 100 characters.");

            RuleFor(x => x.Phone)
                .NotEmpty().WithMessage("Phone is required.")
                .Matches(@"^\+?[0-9]{7,15}$").WithMessage("Invalid phone number.");

            RuleFor(x => x.Skills)
                .NotEmpty().WithMessage("Skills are required.")
                .MaximumLength(500).WithMessage("Skills cannot exceed 500 characters.");

            RuleFor(x => x.WorkExperience)
                .MaximumLength(2000).WithMessage("Work experience cannot exceed 2000 characters.");
        }
    }
}
