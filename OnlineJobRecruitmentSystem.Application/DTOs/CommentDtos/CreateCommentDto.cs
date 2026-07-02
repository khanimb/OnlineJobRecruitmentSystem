namespace OnlineJobRecruitmentSystem.Application.DTOs.CommentDtos
{
    public class CreateCommentDto
    {
        public int JobPostId { get; set; }
        public string Text { get; set; } = string.Empty;
    }
}