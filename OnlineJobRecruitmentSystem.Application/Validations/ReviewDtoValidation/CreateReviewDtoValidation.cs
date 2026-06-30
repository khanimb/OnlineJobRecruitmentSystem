using FluentValidation;
using OnlineJobRecruitmentSystem.Application.DTOs.ReviewDtos;

namespace OnlineJobRecruitmentSystem.Application.Validations.ReviewDtoValidation
{
    public class CreateReviewDtoValidation : AbstractValidator<CreateReviewDto>
    {
        public CreateReviewDtoValidation()
        {
            RuleFor(x => x.RevieweeId)
                .GreaterThan(0).WithMessage("Reviewee is required.");

            RuleFor(x => x.Rating)
                .InclusiveBetween(1, 5).WithMessage("Rating must be between 1 and 5.");

            RuleFor(x => x.Comment)
                .MaximumLength(1000).WithMessage("Comment cannot exceed 1000 characters.");
        }
    }
}