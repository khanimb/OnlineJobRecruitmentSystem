using FluentValidation.TestHelper;
using OnlineJobRecruitmentSystem.Application.DTOs.CommentDtos;
using OnlineJobRecruitmentSystem.Application.Validations.CommentDtoValidation;

namespace OnlineJobRecruitmentSystem.Tests.Validations
{
    public class CreateCommentDtoValidationTests
    {
        private readonly CreateCommentDtoValidation _validator = new();

        [Fact]
        public void ValidDto_PassesValidation()
        {
            var dto = new CreateCommentDto { JobPostId = 1, Text = "Is this remote?" };

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void ZeroJobPostId_FailsValidation()
        {
            var dto = new CreateCommentDto { JobPostId = 0, Text = "Hello" };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.JobPostId);
        }

        [Fact]
        public void EmptyText_FailsValidation()
        {
            var dto = new CreateCommentDto { JobPostId = 1, Text = "" };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Text);
        }

        [Fact]
        public void TooLongText_FailsValidation()
        {
            var dto = new CreateCommentDto { JobPostId = 1, Text = new string('a', 1001) };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Text);
        }
    }
}