using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using OnlineJobRecruitmentSystem.Application.DTOs.ReviewDtos;
using OnlineJobRecruitmentSystem.Application.Interfaces;
using OnlineJobRecruitmentSystem.Domain.Entities;
using OnlineJobRecruitmentSystem.Infrastructure.Data;

namespace OnlineJobRecruitmentSystem.Infrastructure.Services
{
    public class ReviewService : IReviewService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public ReviewService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ReturnReviewDto?> CreateReviewAsync(int reviewerId, CreateReviewDto dto)
        {
            if (reviewerId == dto.RevieweeId)
                return null;

            var revieweeExists = await _context.Users.AnyAsync(u => u.Id == dto.RevieweeId);
            if (!revieweeExists)
                return null;

            var review = new Review
            {
                ReviewerId = reviewerId,
                RevieweeId = dto.RevieweeId,
                Rating = dto.Rating,
                Comment = dto.Comment ?? string.Empty,
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            var reviewer = await _context.Users.FindAsync(reviewerId);
            review.Reviewer = reviewer!;

            return _mapper.Map<ReturnReviewDto>(review);
        }

        public async Task<List<ReturnReviewDto>> GetReviewsForUserAsync(int userId)
        {
            return await _context.Reviews
                .Where(r => r.RevieweeId == userId)
                .ProjectTo<ReturnReviewDto>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        public async Task<List<ReturnReviewDto>> GetReviewsByReviewerAsync(int reviewerId)
        {
            return await _context.Reviews
                .Where(r => r.ReviewerId == reviewerId)
                .ProjectTo<ReturnReviewDto>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        public async Task<double> GetAverageRatingAsync(int userId)
        {
            var query = _context.Reviews.Where(r => r.RevieweeId == userId);
            return await query.AnyAsync() ? await query.AverageAsync(r => r.Rating) : 0;
        }

        public async Task<bool> UpdateReviewAsync(int reviewId, int reviewerId, UpdateReviewDto dto)
        {
            var review = await _context.Reviews
                .FirstOrDefaultAsync(r => r.Id == reviewId && r.ReviewerId == reviewerId);
            if (review == null) return false;

            review.Rating = dto.Rating;
            review.Comment = dto.Comment ?? string.Empty;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteReviewAsync(int reviewId, int reviewerId)
        {
            var review = await _context.Reviews
                .FirstOrDefaultAsync(r => r.Id == reviewId && r.ReviewerId == reviewerId);
            if (review == null) return false;

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}