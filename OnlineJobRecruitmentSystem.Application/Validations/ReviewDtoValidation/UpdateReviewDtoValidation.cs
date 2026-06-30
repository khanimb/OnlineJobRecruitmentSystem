using FluentValidation;
using OnlineJobRecruitmentSystem.Application.DTOs.ReviewDtos;

namespace OnlineJobRecruitmentSystem.Application.Validations.ReviewDtoValidation
{
    public class UpdateReviewDtoValidation : AbstractValidator<UpdateReviewDto>
    {
        public UpdateReviewDtoValidation()
        {
            RuleFor(x => x.Rating)
                .InclusiveBetween(1, 5).WithMessage("Rating must be between 1 and 5.");

            RuleFor(x => x.Comment)
                .MaximumLength(1000).WithMessage("Comment cannot exceed 1000 characters.");
        }
    }
}