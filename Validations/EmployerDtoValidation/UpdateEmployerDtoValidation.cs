using FluentValidation;
using OnlineJobRecruitmentSystem.DTOs.EmployerDtos;

namespace OnlineJobRecruitmentSystem.Validations.EmployerDtoValidation
{
    public class UpdateEmployerDtoValidation : AbstractValidator<UpdateEmployerDto>
    {
        public UpdateEmployerDtoValidation()
        {
            RuleFor(x => x.CompanyName)
                .NotEmpty().WithMessage("Company name is required.")
                .MaximumLength(100).WithMessage("Company name cannot exceed 100 characters.");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required.")
                .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters.");

            RuleFor(x => x.Website)
                .MaximumLength(200).WithMessage("Website cannot exceed 200 characters.");
        }
    }
}
