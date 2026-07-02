using FluentValidation;
using OnlineJobRecruitmentSystem.Application.DTOs.CommentDtos;

namespace OnlineJobRecruitmentSystem.Application.Validations.CommentDtoValidation
{
    public class CreateCommentDtoValidation : AbstractValidator<CreateCommentDto>
    {
        public CreateCommentDtoValidation()
        {
            RuleFor(x => x.JobPostId)
                .GreaterThan(0).WithMessage("Invalid job post.");

            RuleFor(x => x.Text)
                .NotEmpty().WithMessage("Comment cannot be empty.")
                .MaximumLength(1000).WithMessage("Comment cannot exceed 1000 characters.");
        }
    }
}
