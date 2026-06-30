using OnlineJobRecruitmentSystem.Application.DTOs.ReviewDtos;

namespace OnlineJobRecruitmentSystem.Application.Interfaces
{
    public interface IReviewService
    {
        Task<ReturnReviewDto> CreateReviewAsync(int reviewerId, CreateReviewDto dto);
        Task<List<ReturnReviewDto>> GetReviewsForUserAsync(int userId);
        Task<double> GetAverageRatingAsync(int userId);
        Task UpdateReviewAsync(int reviewId, int reviewerId, UpdateReviewDto dto);
        Task DeleteReviewAsync(int reviewId, int reviewerId);
    }
}