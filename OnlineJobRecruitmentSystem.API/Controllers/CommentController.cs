using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineJobRecruitmentSystem.Application.DTOs.CommentDtos;
using OnlineJobRecruitmentSystem.Application.Interfaces;
using OnlineJobRecruitmentSystem.Common;

namespace OnlineJobRecruitmentSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CommentController(
        ICommentService commentService,
        IValidator<CreateCommentDto> createValidator,
        IValidator<UpdateCommentDto> updateValidator) : BaseApiController
    {
        [HttpPost]
        public async Task<IActionResult> CreateComment(CreateCommentDto dto)
        {
            var result = await createValidator.ValidateAsync(dto);
            if (!result.IsValid)
                return BadRequest(ResponseModel<string>.Fail(result.Errors[0].ErrorMessage));

            var created = await commentService.CreateCommentAsync(CurrentUserId, dto);
            if (!created)
                return NotFound(ResponseModel<string>.Fail("Job post not found."));

            return Ok(ResponseModel<string>.Ok(null!, "Comment added."));
        }

        [AllowAnonymous]
        [HttpGet("jobpost/{jobPostId}")]
        public async Task<IActionResult> GetComments(int jobPostId)
        {
            var comments = await commentService.GetCommentsByJobPostAsync(jobPostId);
            return Ok(ResponseModel<List<ReturnCommentDto>>.Ok(comments));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateComment(int id, UpdateCommentDto dto)
        {
            var result = await updateValidator.ValidateAsync(dto);
            if (!result.IsValid)
                return BadRequest(ResponseModel<string>.Fail(result.Errors[0].ErrorMessage));

            var updated = await commentService.UpdateCommentAsync(id, CurrentUserId, dto);
            if (!updated)
                return NotFound(ResponseModel<string>.Fail("Comment not found."));

            return Ok(ResponseModel<string>.Ok(null!, "Comment updated."));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var deleted = await commentService.DeleteCommentAsync(id, CurrentUserId);
            if (!deleted)
                return NotFound(ResponseModel<string>.Fail("Comment not found."));

            return Ok(ResponseModel<string>.Ok(null!, "Comment deleted."));
        }
    }
}