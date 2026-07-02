namespace OnlineJobRecruitmentSystem.Application.DTOs.CommentDtos
{
    public class ReturnCommentDto
    {
        public int Id { get; set; }
        public int JobPostId { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}