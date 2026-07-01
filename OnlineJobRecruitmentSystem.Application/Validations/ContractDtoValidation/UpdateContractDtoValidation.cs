using FluentValidation;
using OnlineJobRecruitmentSystem.Application.DTOs.ContractDtos;

namespace OnlineJobRecruitmentSystem.Application.Validations.ContractDtoValidation
{
    public class UpdateContractDtoValidation : AbstractValidator<UpdateContractDto>
    {
        public UpdateContractDtoValidation()
        {
            RuleFor(x => x.Status)
                .IsInEnum().WithMessage("Invalid contract status.");
        }
    }
}
