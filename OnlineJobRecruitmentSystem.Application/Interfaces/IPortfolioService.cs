using OnlineJobRecruitmentSystem.Application.DTOs.PortfolioDtos;

namespace OnlineJobRecruitmentSystem.Application.Interfaces
{
    public interface IPortfolioService
    {
        Task<ReturnPortfolioItemDto> CreateAsync(int jobSeekerProfileId, CreatePortfolioItemDto dto);
        Task<List<ReturnPortfolioItemDto>> GetByJobSeekerAsync(int jobSeekerProfileId);
        Task UpdateAsync(int itemId, int jobSeekerProfileId, UpdatePortfolioItemDto dto);
        Task DeleteAsync(int itemId, int jobSeekerProfileId);
    }
}