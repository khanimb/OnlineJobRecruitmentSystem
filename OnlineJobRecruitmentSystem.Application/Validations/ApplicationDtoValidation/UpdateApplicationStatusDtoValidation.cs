using FluentValidation;
using OnlineJobRecruitmentSystem.Application.DTOs.ApplicationDtos;

namespace OnlineJobRecruitmentSystem.Application.Validations.ApplicationDtoValidation
{
    public class UpdateApplicationStatusDtoValidation : AbstractValidator<UpdateApplicationStatusDto>
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
