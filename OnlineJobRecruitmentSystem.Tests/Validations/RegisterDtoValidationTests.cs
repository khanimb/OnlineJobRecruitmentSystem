using FluentValidation.TestHelper;
using OnlineJobRecruitmentSystem.Application.DTOs.UserDtos;
using OnlineJobRecruitmentSystem.Application.Validations.UserDtoValidation;

namespace OnlineJobRecruitmentSystem.Tests.Validations
{
    public class RegisterDtoValidationTests
    {
        private readonly RegisterDtoValidation _validator = new();

        [Fact]
        public void ValidDto_PassesValidation()
        {
            var dto = new RegisterDto
            {
                Username = "testuser",
                Email = "test@example.com",
                Password = "Test1234",
                Role = "JobSeeker"
            };

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void EmptyUsername_FailsValidation()
        {
            var dto = new RegisterDto { Username = "", Email = "a@b.com", Password = "Test1234", Role = "JobSeeker" };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Username);
        }

        [Fact]
        public void ShortUsername_FailsValidation()
        {
            var dto = new RegisterDto { Username = "ab", Email = "a@b.com", Password = "Test1234", Role = "JobSeeker" };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Username);
        }

        [Fact]
        public void InvalidEmail_FailsValidation()
        {
            var dto = new RegisterDto { Username = "testuser", Email = "not-an-email", Password = "Test1234", Role = "JobSeeker" };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Email);
        }

        [Fact]
        public void ShortPassword_FailsValidation()
        {
            var dto = new RegisterDto { Username = "testuser", Email = "a@b.com", Password = "123", Role = "JobSeeker" };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Password);
        }

        [Theory]
        [InlineData("Admin")]
        [InlineData("SuperUser")]
        [InlineData("")]
        public void InvalidRole_FailsValidation(string role)
        {
            var dto = new RegisterDto { Username = "testuser", Email = "a@b.com", Password = "Test1234", Role = role };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Role);
        }
    }
}