using Microsoft.AspNetCore.Http;
using OnlineJobRecruitmentSystem.Infrastructure.Extensions;

namespace OnlineJobRecruitmentSystem.Tests.Extensions
{
    public class FileExtensionsTests
    {
        private static IFormFile CreateFile(string fileName, long sizeBytes)
        {
            var stream = new MemoryStream(new byte[sizeBytes]);
            return new FormFile(stream, 0, sizeBytes, "file", fileName);
        }

        [Theory]
        [InlineData("cv.pdf", true)]
        [InlineData("cv.docx", true)]
        [InlineData("cv.doc", true)]
        [InlineData("cv.exe", false)]
        [InlineData("cv.txt", false)]
        public void IsValidType_ChecksExtensionCorrectly(string fileName, bool expected)
        {
            var file = CreateFile(fileName, 100);

            var result = file.IsValidType(".pdf", ".doc", ".docx");

            Assert.Equal(expected, result);
        }

        [Fact]
        public void IsValidType_IsCaseInsensitive()
        {
            var file = CreateFile("CV.PDF", 100);

            var result = file.IsValidType(".pdf");

            Assert.True(result);
        }

        [Fact]
        public void IsValidSize_WithinLimit_ReturnsTrue()
        {
            var file = CreateFile("cv.pdf", 1024);

            var result = file.IsValidSize(2048);

            Assert.True(result);
        }

        [Fact]
        public void IsValidSize_ExceedsLimit_ReturnsFalse()
        {
            var file = CreateFile("cv.pdf", 5 * 1024 * 1024 + 1);

            var result = file.IsValidSize(5 * 1024 * 1024);

            Assert.False(result);
        }
    }
}