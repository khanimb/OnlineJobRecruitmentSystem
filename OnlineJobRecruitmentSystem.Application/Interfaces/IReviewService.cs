using OnlineJobRecruitmentSystem.Application.DTOs.ReviewDtos;

namespace OnlineJobRecruitmentSystem.Application.Interfaces
{
    public interface IReviewService
    {
        Task<ReturnReviewDto?> CreateReviewAsync(int reviewerId, CreateReviewDto dto);
        Task<List<ReturnReviewDto>> GetReviewsForUserAsync(int userId);
        Task<List<ReturnReviewDto>> GetReviewsByReviewerAsync(int reviewerId);
        Task<double> GetAverageRatingAsync(int userId);
        Task<bool> UpdateReviewAsync(int reviewId, int reviewerId, UpdateReviewDto dto);
        Task<bool> DeleteReviewAsync(int reviewId, int reviewerId);
    }
}