using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineJobRecruitmentSystem.Application.DTOs.ReviewDtos;
using OnlineJobRecruitmentSystem.Application.Interfaces;
using OnlineJobRecruitmentSystem.Common;

namespace OnlineJobRecruitmentSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReviewController(
        IReviewService reviewService,
        IValidator<CreateReviewDto> createValidator,
        IValidator<UpdateReviewDto> updateValidator) : BaseApiController
    {
        

        [HttpPost]
        public async Task<IActionResult> CreateReview(CreateReviewDto dto)
        {
            var result = await createValidator.ValidateAsync(dto);
            if (!result.IsValid)
                return BadRequest(ResponseModel<string>.Fail(result.Errors[0].ErrorMessage));

            var created = await reviewService.CreateReviewAsync(CurrentUserId, dto);
            if (created == null)
                return BadRequest(ResponseModel<string>.Fail("Invalid review target."));

            return Ok(ResponseModel<string>.Ok(null!, "Review created."));
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetReviews(int userId)
        {
            var reviews = await reviewService.GetReviewsForUserAsync(userId);
            return Ok(ResponseModel<List<ReturnReviewDto>>.Ok(reviews));
        }

        [HttpGet("my")]
        public async Task<IActionResult> GetMyReviews()
        {
            var reviews = await reviewService.GetReviewsByReviewerAsync(CurrentUserId);
            return Ok(ResponseModel<List<ReturnReviewDto>>.Ok(reviews));
        }

        [HttpGet("user/{userId}/rating")]
        public async Task<IActionResult> GetAverageRating(int userId)
        {
            var avg = await reviewService.GetAverageRatingAsync(userId);
            return Ok(ResponseModel<double>.Ok(avg));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateReview(int id, UpdateReviewDto dto)
        {
            var result = await updateValidator.ValidateAsync(dto);
            if (!result.IsValid)
                return BadRequest(ResponseModel<string>.Fail(result.Errors[0].ErrorMessage));

            var updated = await reviewService.UpdateReviewAsync(id, CurrentUserId, dto);
            if (!updated)
                return NotFound(ResponseModel<string>.Fail("Review not found."));

            return Ok(ResponseModel<string>.Ok(null!, "Review updated."));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var deleted = await reviewService.DeleteReviewAsync(id, CurrentUserId);
            if (!deleted)
                return NotFound(ResponseModel<string>.Fail("Review not found."));

            return Ok(ResponseModel<string>.Ok(null!, "Review deleted."));
        }
    }
}