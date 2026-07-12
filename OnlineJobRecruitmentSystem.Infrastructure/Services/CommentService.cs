using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using OnlineJobRecruitmentSystem.Application.DTOs.CommentDtos;
using OnlineJobRecruitmentSystem.Application.Interfaces;
using OnlineJobRecruitmentSystem.Domain.Entities;
using OnlineJobRecruitmentSystem.Infrastructure.Data;

namespace OnlineJobRecruitmentSystem.Infrastructure.Services
{
    public class CommentService : ICommentService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public CommentService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<bool> CreateCommentAsync(int userId, CreateCommentDto dto)
        {
            var jobPostExists = await _context.JobPosts.AnyAsync(j => j.Id == dto.JobPostId);
            if (!jobPostExists) return false;

            var comment = new Comment
            {
                JobPostId = dto.JobPostId,
                UserId = userId,
                Text = dto.Text
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<ReturnCommentDto>> GetCommentsByJobPostAsync(int jobPostId)
        {
            return await _context.Comments
                .Where(c => c.JobPostId == jobPostId)
                .OrderByDescending(c => c.CreatedAt)
                .ProjectTo<ReturnCommentDto>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        public async Task<bool> UpdateCommentAsync(int commentId, int userId, UpdateCommentDto dto)
        {
            var comment = await _context.Comments
                .FirstOrDefaultAsync(c => c.Id == commentId && c.UserId == userId);
            if (comment == null) return false;

            comment.Text = dto.Text;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteCommentAsync(int commentId, int userId)
        {
            var comment = await _context.Comments
                .FirstOrDefaultAsync(c => c.Id == commentId && c.UserId == userId);
            if (comment == null) return false;

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}