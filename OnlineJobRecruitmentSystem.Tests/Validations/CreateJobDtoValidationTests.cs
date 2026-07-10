using FluentValidation.TestHelper;
using OnlineJobRecruitmentSystem.Application.DTOs.JobDtos;
using OnlineJobRecruitmentSystem.Application.Validations.JobDtoValidation;
using Xunit;

namespace OnlineJobRecruitmentSystem.Tests.Validations
{
    public class CreateJobDtoValidationTests
    {
        private readonly CreateJobDtoValidation _validator = new();

        private static CreateJobDto ValidDto() => new()
        {
            Title = "Backend Developer",
            Description = "We are looking for a .NET developer.",
            Location = "Baku",
            JobType = "FullTime",
            SalaryMin = 1000,
            SalaryMax = 2000,
            Deadline = DateTime.UtcNow.AddMonths(1)
        };

        [Fact]
        public void ValidDto_PassesValidation()
        {
            var result = _validator.TestValidate(ValidDto());

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void EmptyTitle_FailsValidation()
        {
            var dto = ValidDto();
            dto.Title = "";

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Title);
        }

        [Theory]
        [InlineData("Full-time")]
        [InlineData("Contract")]
        [InlineData("")]
        public void InvalidJobType_FailsValidation(string jobType)
        {
            var dto = ValidDto();
            dto.JobType = jobType;

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.JobType);
        }

        [Fact]
        public void SalaryMinZeroOrLess_FailsValidation()
        {
            var dto = ValidDto();
            dto.SalaryMin = 0;

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.SalaryMin);
        }

        [Fact]
        public void SalaryMaxLessThanSalaryMin_FailsValidation()
        {
            var dto = ValidDto();
            dto.SalaryMin = 2000;
            dto.SalaryMax = 1000;

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.SalaryMax);
        }

        [Fact]
        public void PastDeadline_FailsValidation()
        {
            var dto = ValidDto();
            dto.Deadline = DateTime.UtcNow.AddDays(-1);

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Deadline);
        }
    }
}