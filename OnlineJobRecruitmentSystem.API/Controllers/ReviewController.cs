using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineJobRecruitmentSystem.Application.DTOs.ReviewDtos;
using OnlineJobRecruitmentSystem.Common;
using OnlineJobRecruitmentSystem.Domain.Entities;
using OnlineJobRecruitmentSystem.Infrastructure.Data;
using System.Security.Claims;

namespace OnlineJobRecruitmentSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReviewController(
        AppDbContext context,
        IValidator<CreateReviewDto> createValidator,
        IValidator<UpdateReviewDto> updateValidator) : ControllerBase
    {
        private int GetUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        [HttpPost]
        public async Task<IActionResult> CreateReview(CreateReviewDto dto)
        {
            var result = await createValidator.ValidateAsync(dto);
            if (!result.IsValid)
                return BadRequest(ResponseModel<string>.Fail(result.Errors[0].ErrorMessage));

            var reviewerId = GetUserId();

            var review = new Review
            {
                ReviewerId = reviewerId,
                RevieweeId = dto.RevieweeId,
                Rating = dto.Rating,
                Comment = dto.Comment ?? string.Empty
            };

            context.Reviews.Add(review);
            await context.SaveChangesAsync();

            return Ok(ResponseModel<string>.Ok(null!, "Review created."));
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetReviews(int userId)
        {
            var reviews = await context.Reviews
                .Include(r => r.Reviewer)
                .Where(r => r.RevieweeId == userId)
                .Select(r => new ReturnReviewDto
                {
                    Id = r.Id,
                    ReviewerId = r.ReviewerId,
                    ReviewerName = r.Reviewer.Email,
                    RevieweeId = r.RevieweeId,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();

            return Ok(ResponseModel<List<ReturnReviewDto>>.Ok(reviews));
        }

        [HttpGet("my")]
        public async Task<IActionResult> GetMyReviews()
        {
            var userId = GetUserId();

            var reviews = await context.Reviews
                .Include(r => r.Reviewee)
                .Where(r => r.ReviewerId == userId)
                .Select(r => new ReturnReviewDto
                {
                    Id = r.Id,
                    ReviewerId = r.ReviewerId,
                    RevieweeId = r.RevieweeId,
                    ReviewerName = r.Reviewer.Email,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();

            return Ok(ResponseModel<List<ReturnReviewDto>>.Ok(reviews));
        }

        [HttpGet("user/{userId}/rating")]
        public async Task<IActionResult> GetAverageRating(int userId)
        {
            var reviews = await context.Reviews
                .Where(r => r.RevieweeId == userId)
                .ToListAsync();

            var avg = reviews.Any() ? reviews.Average(r => r.Rating) : 0;
            return Ok(ResponseModel<double>.Ok(avg));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateReview(int id, UpdateReviewDto dto)
        {
            var result = await updateValidator.ValidateAsync(dto);
            if (!result.IsValid)
                return BadRequest(ResponseModel<string>.Fail(result.Errors[0].ErrorMessage));

            var reviewerId = GetUserId();

            var review = await context.Reviews
                .FirstOrDefaultAsync(r => r.Id == id && r.ReviewerId == reviewerId);

            if (review == null)
                return NotFound(ResponseModel<string>.Fail("Review not found."));

            review.Rating = dto.Rating;
            review.Comment = dto.Comment ?? string.Empty;
            await context.SaveChangesAsync();

            return Ok(ResponseModel<string>.Ok(null!, "Review updated."));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var reviewerId = GetUserId();

            var review = await context.Reviews
                .FirstOrDefaultAsync(r => r.Id == id && r.ReviewerId == reviewerId);

            if (review == null)
                return NotFound(ResponseModel<string>.Fail("Review not found."));

            context.Reviews.Remove(review);
            await context.SaveChangesAsync();

            return Ok(ResponseModel<string>.Ok(null!, "Review deleted."));
        }
    }
}