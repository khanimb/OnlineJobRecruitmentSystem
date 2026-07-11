using AutoMapper;
using AutoMapper.QueryableExtensions;
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
        private readonly IMapper _mapper;

        public PortfolioService(AppDbContext context, FileManager fileManager, IMapper mapper)
        {
            _context = context;
            _fileManager = fileManager;
            _mapper = mapper;
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

            return _mapper.Map<ReturnPortfolioItemDto>(item);
        }

        public async Task<List<ReturnPortfolioItemDto>> GetByJobSeekerAsync(int jobSeekerProfileId)
        {
            return await _context.PortfolioItems
                .Where(p => p.JobSeekerProfileId == jobSeekerProfileId)
                .ProjectTo<ReturnPortfolioItemDto>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        public async Task<bool> UpdateAsync(int itemId, int jobSeekerProfileId, UpdatePortfolioItemDto dto)
        {
            var item = await _context.PortfolioItems
                .FirstOrDefaultAsync(p => p.Id == itemId && p.JobSeekerProfileId == jobSeekerProfileId);
            if (item == null) return false;

            item.Title = dto.Title ?? string.Empty;
            item.Description = dto.Description ?? string.Empty;

            if (dto.File != null)
            {
                _fileManager.Delete(item.FileUrl);
                item.FileUrl = await _fileManager.UploadAsync(dto.File, "portfolio");
                item.FileType = Path.GetExtension(dto.File.FileName);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int itemId, int jobSeekerProfileId)
        {
            var item = await _context.PortfolioItems
                .FirstOrDefaultAsync(p => p.Id == itemId && p.JobSeekerProfileId == jobSeekerProfileId);
            if (item == null) return false;

            _fileManager.Delete(item.FileUrl);
            _context.PortfolioItems.Remove(item);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}