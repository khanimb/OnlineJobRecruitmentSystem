using FluentValidation;
using OnlineJobRecruitmentSystem.Application.DTOs.PortfolioDtos;

namespace OnlineJobRecruitmentSystem.Application.Validations.PortfolioDtoValidation
{
    public class CreatePortfolioItemDtoValidation : AbstractValidator<CreatePortfolioItemDto>
    {
        public CreatePortfolioItemDtoValidation()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required.")
                .MaximumLength(200).WithMessage("Title cannot exceed 200 characters.");

            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters.");

            RuleFor(x => x.File)
                .NotNull().WithMessage("File is required.");
        }
    }
}