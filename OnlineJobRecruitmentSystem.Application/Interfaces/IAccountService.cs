using OnlineJobRecruitmentSystem.Application.DTOs.AccountDtos;

namespace OnlineJobRecruitmentSystem.Application.Interfaces
{
    public class CvPdfResult
    {
        public byte[] PdfBytes { get; set; } = Array.Empty<byte>();
        public string FileName { get; set; } = string.Empty;
    }

    public interface IAccountService
    {
        Task<ProfileReturnDto?> GetProfileAsync(int userId);
        Task<CvPdfResult?> GenerateCvPdfAsync(int userId);
    }
}