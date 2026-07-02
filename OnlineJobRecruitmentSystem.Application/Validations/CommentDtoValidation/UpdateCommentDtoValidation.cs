using FluentValidation;
using OnlineJobRecruitmentSystem.Application.DTOs.CommentDtos;

namespace OnlineJobRecruitmentSystem.Application.Validations.CommentDtoValidation
{
    public class UpdateCommentDtoValidation : AbstractValidator<UpdateCommentDto>
    {
        public UpdateCommentDtoValidation()
        {
            RuleFor(x => x.Text)
                .NotEmpty().WithMessage("Comment cannot be empty.")
                .MaximumLength(1000).WithMessage("Comment cannot exceed 1000 characters.");
        }
    }
}
