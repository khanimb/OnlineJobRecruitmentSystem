using FluentValidation;
using OnlineJobRecruitmentSystem.Application.DTOs.ContractDtos;

namespace OnlineJobRecruitmentSystem.Application.Validations.ContractDtoValidation
{
    public class CreateContractDtoValidation : AbstractValidator<CreateContractDto>
    {
        public CreateContractDtoValidation()
        {
            RuleFor(x => x.JobPostId)
                .GreaterThan(0).WithMessage("Job post is required.");

            RuleFor(x => x.JobSeekerProfileId)
                .GreaterThan(0).WithMessage("Job seeker is required.");

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("Amount must be greater than 0.");
        }
    }
}