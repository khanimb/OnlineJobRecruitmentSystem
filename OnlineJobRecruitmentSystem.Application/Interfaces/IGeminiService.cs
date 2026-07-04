namespace OnlineJobRecruitmentSystem.Application.Interfaces
{
    public interface IGeminiService
    {
        Task<string> GenerateContentAsync(string prompt);
    }
}
