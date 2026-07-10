using FluentValidation;
using OnlineJobRecruitmentSystem.Application.DTOs.UserDtos;

namespace OnlineJobRecruitmentSystem.Application.Validations.UserDtoValidation
{
    public class RefreshTokenDtoValidation : AbstractValidator<RefreshTokenDto>
    {
        public RefreshTokenDtoValidation()
        {
            RuleFor(x => x.RefreshToken)
                .NotEmpty().WithMessage("Refresh token cannot be empty.");
        }
    }
}