using OnlineJobRecruitmentSystem.Application.DTOs.CommentDtos;

namespace OnlineJobRecruitmentSystem.Application.Interfaces
{
    public interface ICommentService
    {
        Task<bool> CreateCommentAsync(int userId, CreateCommentDto dto);
        Task<List<ReturnCommentDto>> GetCommentsByJobPostAsync(int jobPostId);
        Task<bool> UpdateCommentAsync(int commentId, int userId, UpdateCommentDto dto);
        Task<bool> DeleteCommentAsync(int commentId, int userId);
    }
}