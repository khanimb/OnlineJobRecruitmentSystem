using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineJobRecruitmentSystem.Application.DTOs.CommentDtos;
using OnlineJobRecruitmentSystem.Common;
using OnlineJobRecruitmentSystem.Domain.Entities;
using OnlineJobRecruitmentSystem.Infrastructure.Data;

namespace OnlineJobRecruitmentSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CommentController(
        AppDbContext context,
        IValidator<CreateCommentDto> createValidator,
        IValidator<UpdateCommentDto> updateValidator) : BaseApiController
    {
        

        [HttpPost]
        public async Task<IActionResult> CreateComment(CreateCommentDto dto)
        {
            var result = await createValidator.ValidateAsync(dto);
            if (!result.IsValid)
                return BadRequest(ResponseModel<string>.Fail(result.Errors[0].ErrorMessage));

            var jobPostExists = await context.JobPosts.AnyAsync(j => j.Id == dto.JobPostId);
            if (!jobPostExists)
                return NotFound(ResponseModel<string>.Fail("Job post not found."));

            var comment = new Comment
            {
                JobPostId = dto.JobPostId,
                UserId = CurrentUserId,
                Text = dto.Text
            };

            context.Comments.Add(comment);
            await context.SaveChangesAsync();

            return Ok(ResponseModel<string>.Ok(null!, "Comment added."));
        }

        [HttpGet("jobpost/{jobPostId}")]
        public async Task<IActionResult> GetComments(int jobPostId)
        {
            var comments = await context.Comments
                .Where(c => c.JobPostId == jobPostId)
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new ReturnCommentDto
                {
                    Id = c.Id,
                    JobPostId = c.JobPostId,
                    UserId = c.UserId,
                    Username = c.User.Username,
                    Text = c.Text,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();

            return Ok(ResponseModel<List<ReturnCommentDto>>.Ok(comments));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateComment(int id, UpdateCommentDto dto)
        {
            var result = await updateValidator.ValidateAsync(dto);
            if (!result.IsValid)
                return BadRequest(ResponseModel<string>.Fail(result.Errors[0].ErrorMessage));

            var userId = CurrentUserId;
            var comment = await context.Comments
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (comment == null)
                return NotFound(ResponseModel<string>.Fail("Comment not found."));

            comment.Text = dto.Text;
            await context.SaveChangesAsync();

            return Ok(ResponseModel<string>.Ok(null!, "Comment updated."));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var userId = CurrentUserId;
            var comment = await context.Comments
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (comment == null)
                return NotFound(ResponseModel<string>.Fail("Comment not found."));

            context.Comments.Remove(comment);
            await context.SaveChangesAsync();

            return Ok(ResponseModel<string>.Ok(null!, "Comment deleted."));
        }
    }
}