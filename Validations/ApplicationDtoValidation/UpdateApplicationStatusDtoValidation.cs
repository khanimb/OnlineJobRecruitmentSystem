using FluentValidation;
using OnlineJobRecruitmentSystem.DTOs.ApplicationDtos;

namespace OnlineJobRecruitmentSystem.Validations.ApplicationDtoValidation
{
    public class UpdateApplicationStatusDtoValidation : AbstractValidator<UpdateApplicationStatusDto>
    {
        public UpdateApplicationStatusDtoValidation()
        {
            RuleFor(x => x.Status)
                .NotEmpty().WithMessage("Status is required.")
                .Must(s => new[] { "Applied", "Reviewed", "Shortlisted", "Rejected" }.Contains(s))
                .WithMessage("Status must be: Applied, Reviewed, Shortlisted or Rejected.");

            RuleFor(x => x.Notes)
                .MaximumLength(500).WithMessage("Notes cannot exceed 500 characters.");
        }
    }
}
