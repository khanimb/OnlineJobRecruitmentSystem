using FluentValidation;
using OnlineJobRecruitmentSystem.Application.DTOs.JobApplicationDtos;

namespace OnlineJobRecruitmentSystem.Application.Validations.ApplicationDtoValidation
{
    public class UpdateApplicationStatusDtoValidation : AbstractValidator<UpdateJobApplicationStatusDto>
    {
        public UpdateApplicationStatusDtoValidation()
        {
            RuleFor(x => x.Status)
                .IsInEnum().WithMessage("Status must be: Applied, Reviewed, Shortlisted or Rejected.");

            RuleFor(x => x.Notes)
                .MaximumLength(500).WithMessage("Notes cannot exceed 500 characters.");
        }
    }
}
