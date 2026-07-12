using FluentValidation;
using OnlineJobRecruitmentSystem.Application.DTOs.PaymentDtos;

namespace OnlineJobRecruitmentSystem.Application.Validations.PaymentDtoValidation
{
    public class CreatePaymentDtoValidation : AbstractValidator<CreatePaymentDto>
    {
        public CreatePaymentDtoValidation()
        {
            RuleFor(x => x.Plan)
                .NotEmpty().WithMessage("Plan is required.")
                .Must(p => p == "basic" || p == "standard" || p == "premium")
                .WithMessage("Plan must be 'basic', 'standard' or 'premium'.");
        }
    }
}