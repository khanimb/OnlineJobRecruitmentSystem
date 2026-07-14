using FluentValidation.TestHelper;
using OnlineJobRecruitmentSystem.Application.DTOs.EmployerDtos;
using OnlineJobRecruitmentSystem.Application.Validations.EmployerDtoValidation;
using Xunit;

namespace OnlineJobRecruitmentSystem.Tests.Validations
{
    public class CreateEmployerDtoValidationTests
    {
        private readonly CreateEmployerDtoValidation _validator = new();

        private static CreateEmployerDto ValidDto() => new()
        {
            CompanyName = "Acme Inc.",
            Description = "We build great software.",
            Website = "https://acme.com"
        };

        [Fact]
        public void ValidDto_PassesValidation()
        {
            var result = _validator.TestValidate(ValidDto());

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void EmptyCompanyName_FailsValidation()
        {
            var dto = ValidDto();
            dto.CompanyName = "";

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.CompanyName);
        }

        [Fact]
        public void CompanyNameTooLong_FailsValidation()
        {
            var dto = ValidDto();
            dto.CompanyName = new string('a', 101);

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.CompanyName);
        }

        [Fact]
        public void EmptyDescription_FailsValidation()
        {
            var dto = ValidDto();
            dto.Description = "";

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Description);
        }

        [Fact]
        public void WebsiteTooLong_FailsValidation()
        {
            var dto = ValidDto();
            dto.Website = "https://" + new string('a', 200) + ".com";

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Website);
        }
    }
}