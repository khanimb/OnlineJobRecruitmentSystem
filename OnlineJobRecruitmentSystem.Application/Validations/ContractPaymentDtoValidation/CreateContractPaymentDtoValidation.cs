using FluentValidation;
using OnlineJobRecruitmentSystem.Application.DTOs.ContractPaymentDtos;

namespace OnlineJobRecruitmentSystem.Application.Validations.ContractPaymentDtoValidation
{
    public class CreateContractPaymentDtoValidation : AbstractValidator<CreateContractPaymentDto>
    {
        public CreateContractPaymentDtoValidation()
        {
            RuleFor(x => x.ContractId)
                .GreaterThan(0).WithMessage("Contract is required.");
        }
    }
}
