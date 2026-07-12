using FluentValidation;
using OnlineJobRecruitmentSystem.Application.DTOs.JobApplicationDtos;

namespace OnlineJobRecruitmentSystem.Application.Validations.ApplicationDtoValidation
{
    public class SendMessageDtoValidation : AbstractValidator<CreateJobApplicationDto>
    {
        public SendMessageDtoValidation()
        {
            RuleFor(x => x.CoverLetter)
                .NotEmpty().WithMessage("Cover letter is required.")
                .MaximumLength(2000).WithMessage("Cover letter cannot exceed 2000 characters.");
        }
    }
}