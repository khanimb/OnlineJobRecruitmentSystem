using FluentValidation;
using OnlineJobRecruitmentSystem.Application.DTOs.MessageDtos;

namespace OnlineJobRecruitmentSystem.Application.Validations.MessageDtoValidation
{
    public class SendMessageDtoValidation : AbstractValidator<SendMessageDto>
    {
        public SendMessageDtoValidation()
        {
            RuleFor(x => x.ReceiverId)
                .GreaterThan(0).WithMessage("Receiver is required.");

            RuleFor(x => x.Content)
                .NotEmpty().WithMessage("Message content is required.")
                .MaximumLength(2000).WithMessage("Message cannot exceed 2000 characters.");
        }
    }
}