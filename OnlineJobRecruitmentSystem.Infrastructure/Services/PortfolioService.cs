using Microsoft.EntityFrameworkCore;
using OnlineJobRecruitmentSystem.Application.DTOs.PortfolioDtos;
using OnlineJobRecruitmentSystem.Application.Interfaces;
using OnlineJobRecruitmentSystem.Domain.Entities;
using OnlineJobRecruitmentSystem.Infrastructure.Data;
using OnlineJobRecruitmentSystem.Infrastructure.Extensions;

namespace OnlineJobRecruitmentSystem.Infrastructure.Services
{
    public class PortfolioService : IPortfolioService
    {
        private readonly AppDbContext _context;
        private readonly FileManager _fileManager;

        public PortfolioService(AppDbContext context, FileManager fileManager)
        {
            _context = context;
            _fileManager = fileManager;
        }

        public async Task<ReturnPortfolioItemDto> CreateAsync(int jobSeekerProfileId, CreatePortfolioItemDto dto)
        {
            var fileUrl = await _fileManager.UploadAsync(dto.File!, "portfolio");

            var item = new PortfolioItem
            {
                JobSeekerProfileId = jobSeekerProfileId,
                Title = dto.Title ?? string.Empty,
                Description = dto.Description ?? string.Empty,
                FileUrl = fileUrl,
                FileType = Path.GetExtension(dto.File?.FileName ?? string.Empty)
            };

            _context.PortfolioItems.Add(item);
            await _context.SaveChangesAsync();

            return MapToDto(item);
        }

        public async Task<List<ReturnPortfolioItemDto>> GetByJobSeekerAsync(int jobSeekerProfileId)
        {
            return await _context.PortfolioItems
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
        }

        public async Task UpdateAsync(int itemId, int jobSeekerProfileId, UpdatePortfolioItemDto dto)
        {
            var item = await _context.PortfolioItems
                .FirstOrDefaultAsync(p => p.Id == itemId && p.JobSeekerProfileId == jobSeekerProfileId);

            if (item != null)
            {
                item.Title = dto.Title ?? string.Empty;
                item.Description = dto.Description ?? string.Empty;

                if (dto.File != null)
                {
                    _fileManager.Delete(item.FileUrl);
                    item.FileUrl = await _fileManager.UploadAsync(dto.File, "portfolio");
                    item.FileType = Path.GetExtension(dto.File.FileName);
                }

                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(int itemId, int jobSeekerProfileId)
        {
            var item = await _context.PortfolioItems
                .FirstOrDefaultAsync(p => p.Id == itemId && p.JobSeekerProfileId == jobSeekerProfileId);

            if (item != null)
            {
                _fileManager.Delete(item.FileUrl);
                _context.PortfolioItems.Remove(item);
                await _context.SaveChangesAsync();
            }
        }

        private static ReturnPortfolioItemDto MapToDto(PortfolioItem item) => new ReturnPortfolioItemDto
        {
            Id = item.Id,
            Title = item.Title,
            Description = item.Description,
            FileUrl = item.FileUrl,
            FileType = item.FileType,
            CreatedAt = item.CreatedAt
        };
    }
}