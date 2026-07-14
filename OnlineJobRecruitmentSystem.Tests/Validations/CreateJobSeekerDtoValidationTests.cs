using FluentValidation.TestHelper;
using OnlineJobRecruitmentSystem.Application.DTOs.JobSeekerDtos;
using OnlineJobRecruitmentSystem.Application.Validations.JobSeekerDtoValidation;
using Xunit;

namespace OnlineJobRecruitmentSystem.Tests.Validations
{
    public class CreateJobSeekerDtoValidationTests
    {
        private readonly CreateJobSeekerDtoValidation _validator = new();

        private static CreateJobSeekerDto ValidDto() => new()
        {
            FullName = "Jane Doe",
            Phone = "+994501234567",
            Skills = "C#, .NET, SQL",
            WorkExperience = "3 years as backend developer."
        };

        [Fact]
        public void ValidDto_PassesValidation()
        {
            var result = _validator.TestValidate(ValidDto());

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void EmptyFullName_FailsValidation()
        {
            var dto = ValidDto();
            dto.FullName = "";

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.FullName);
        }

        [Theory]
        [InlineData("")]
        [InlineData("abc123")]
        [InlineData("123")]
        public void InvalidPhone_FailsValidation(string phone)
        {
            var dto = ValidDto();
            dto.Phone = phone;

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Phone);
        }

        [Fact]
        public void EmptySkills_FailsValidation()
        {
            var dto = ValidDto();
            dto.Skills = "";

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Skills);
        }

        [Fact]
        public void WorkExperienceTooLong_FailsValidation()
        {
            var dto = ValidDto();
            dto.WorkExperience = new string('a', 2001);

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.WorkExperience);
        }
    }
}