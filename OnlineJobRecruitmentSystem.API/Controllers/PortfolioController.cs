using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineJobRecruitmentSystem.Application.DTOs.PortfolioDtos;
using OnlineJobRecruitmentSystem.Common;
using OnlineJobRecruitmentSystem.Domain.Entities;
using OnlineJobRecruitmentSystem.Infrastructure.Data;
using OnlineJobRecruitmentSystem.Infrastructure.Extensions;
using System.Security.Claims;

namespace OnlineJobRecruitmentSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "JobSeeker")]
    public class PortfolioController(
        AppDbContext context,
        FileManager fileManager) : ControllerBase
    {
        private int GetUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        private async Task<int> GetProfileId()
        {
            var userId = GetUserId();
            var profile = await context.JobSeekerProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId);
            return profile?.Id ?? 0;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CreatePortfolioItemDto dto)
        {
            if (dto.File == null || dto.File.Length == 0)
                return BadRequest(ResponseModel<string>.Fail("File is required."));

            var profileId = await GetProfileId();
            if (profileId == 0)
                return NotFound(ResponseModel<string>.Fail("Profile not found."));

            var fileUrl = await fileManager.UploadAsync(dto.File, "portfolio");

            var item = new PortfolioItem
            {
                JobSeekerProfileId = profileId,
                Title = dto.Title ?? string.Empty,
                Description = dto.Description ?? string.Empty,
                FileUrl = fileUrl,
                FileType = Path.GetExtension(dto.File.FileName)
            };

            context.PortfolioItems.Add(item);
            await context.SaveChangesAsync();

            return Ok(ResponseModel<ReturnPortfolioItemDto>.Ok(new ReturnPortfolioItemDto
            {
                Id = item.Id,
                Title = item.Title,
                Description = item.Description,
                FileUrl = item.FileUrl,
                FileType = item.FileType,
                CreatedAt = item.CreatedAt
            }, "Portfolio item created."));
        }

        [HttpGet]
        public async Task<IActionResult> GetMyPortfolio()
        {
            var profileId = await GetProfileId();

            var items = await context.PortfolioItems
                .Where(p => p.JobSeekerProfileId == profileId)
                .Select(p => new ReturnPortfolioItemDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    Description = p.Description,
                    FileUrl = p.FileUrl,
                    FileType = p.FileType,
                    CreatedAt = p.CreatedAt
                })
                .ToListAsync();

            return Ok(ResponseModel<List<ReturnPortfolioItemDto>>.Ok(items));
        }

        [HttpGet("{jobSeekerProfileId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPortfolio(int jobSeekerProfileId)
        {
            var items = await context.PortfolioItems
                .Where(p => p.JobSeekerProfileId == jobSeekerProfileId)
                .Select(p => new ReturnPortfolioItemDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    Description = p.Description,
                    FileUrl = p.FileUrl,
                    FileType = p.FileType,
                    CreatedAt = p.CreatedAt
                })
                .ToListAsync();

            return Ok(ResponseModel<List<ReturnPortfolioItemDto>>.Ok(items));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromForm] UpdatePortfolioItemDto dto)
        {
            var profileId = await GetProfileId();

            var item = await context.PortfolioItems
                .FirstOrDefaultAsync(p => p.Id == id && p.JobSeekerProfileId == profileId);

            if (item == null)
                return NotFound(ResponseModel<string>.Fail("Portfolio item not found."));

            item.Title = dto.Title ?? string.Empty;
            item.Description = dto.Description ?? string.Empty;

            if (dto.File != null)
            {
                fileManager.Delete(item.FileUrl);
                item.FileUrl = await fileManager.UploadAsync(dto.File, "portfolio");
                item.FileType = Path.GetExtension(dto.File.FileName);
            }

            await context.SaveChangesAsync();

            return Ok(ResponseModel<string>.Ok(null!, "Portfolio item updated."));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var profileId = await GetProfileId();

            var item = await context.PortfolioItems
                .FirstOrDefaultAsync(p => p.Id == id && p.JobSeekerProfileId == profileId);

            if (item == null)
                return NotFound(ResponseModel<string>.Fail("Portfolio item not found."));

            fileManager.Delete(item.FileUrl);
            context.PortfolioItems.Remove(item);
            await context.SaveChangesAsync();

            return Ok(ResponseModel<string>.Ok(null!, "Portfolio item deleted."));
        }
    }
}