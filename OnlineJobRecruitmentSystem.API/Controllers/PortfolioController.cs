using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineJobRecruitmentSystem.Application.DTOs.PortfolioDtos;
using OnlineJobRecruitmentSystem.Application.Interfaces;
using OnlineJobRecruitmentSystem.Common;
using OnlineJobRecruitmentSystem.Infrastructure.Data;
using OnlineJobRecruitmentSystem.Infrastructure.Extensions;

namespace OnlineJobRecruitmentSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = OnlineJobRecruitmentSystem.Domain.Common.Roles.JobSeeker)]
    public class PortfolioController(
        AppDbContext context,
        IPortfolioService portfolioService,
        IValidator<CreatePortfolioItemDto> createValidator,
        IValidator<UpdatePortfolioItemDto> updateValidator) : BaseApiController
    {
        private async Task<int> GetProfileId()
        {
            var userId = CurrentUserId;
            var profile = await context.JobSeekerProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId);
            return profile?.Id ?? 0;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CreatePortfolioItemDto dto)
        {
            var result = await createValidator.ValidateAsync(dto);
            if (!result.IsValid)
                return BadRequest(ResponseModel<string>.Fail(result.Errors[0].ErrorMessage));

            if (dto.File == null || dto.File.Length == 0)
                return BadRequest(ResponseModel<string>.Fail("File is required."));

            if (!dto.File.IsValidType(".jpg", ".jpeg", ".png"))
                return BadRequest(ResponseModel<string>.Fail("Only JPG, PNG files are allowed."));

            if (!dto.File.IsValidSize(5 * 1024 * 1024))
                return BadRequest(ResponseModel<string>.Fail("File size must not exceed 5 MB."));

            if (!dto.File.HasValidSignature(".jpg", ".jpeg", ".png"))
                return BadRequest(ResponseModel<string>.Fail("File content does not match its extension."));

            var profileId = await GetProfileId();
            if (profileId == 0)
                return NotFound(ResponseModel<string>.Fail("Profile not found."));

            var item = await portfolioService.CreateAsync(profileId, dto);
            return Ok(ResponseModel<ReturnPortfolioItemDto>.Ok(item, "Portfolio item created."));
        }

        [HttpGet]
        public async Task<IActionResult> GetMyPortfolio()
        {
            var profileId = await GetProfileId();
            var items = await portfolioService.GetByJobSeekerAsync(profileId);
            return Ok(ResponseModel<List<ReturnPortfolioItemDto>>.Ok(items));
        }

        [HttpGet("{jobSeekerProfileId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPortfolio(int jobSeekerProfileId)
        {
            var items = await portfolioService.GetByJobSeekerAsync(jobSeekerProfileId);
            return Ok(ResponseModel<List<ReturnPortfolioItemDto>>.Ok(items));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromForm] UpdatePortfolioItemDto dto)
        {
            var result = await updateValidator.ValidateAsync(dto);
            if (!result.IsValid)
                return BadRequest(ResponseModel<string>.Fail(result.Errors[0].ErrorMessage));

            if (dto.File != null)
            {
                if (!dto.File.IsValidType(".jpg", ".jpeg", ".png"))
                    return BadRequest(ResponseModel<string>.Fail("Only JPG, PNG files are allowed."));

                if (!dto.File.IsValidSize(5 * 1024 * 1024))
                    return BadRequest(ResponseModel<string>.Fail("File size must not exceed 5 MB."));

                if (!dto.File.HasValidSignature(".jpg", ".jpeg", ".png"))
                    return BadRequest(ResponseModel<string>.Fail("File content does not match its extension."));
            }

            var profileId = await GetProfileId();
            var updated = await portfolioService.UpdateAsync(id, profileId, dto);
            if (!updated)
                return NotFound(ResponseModel<string>.Fail("Portfolio item not found."));

            return Ok(ResponseModel<string>.Ok(null!, "Portfolio item updated."));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var profileId = await GetProfileId();
            var deleted = await portfolioService.DeleteAsync(id, profileId);
            if (!deleted)
                return NotFound(ResponseModel<string>.Fail("Portfolio item not found."));

            return Ok(ResponseModel<string>.Ok(null!, "Portfolio item deleted."));
        }
    }
}