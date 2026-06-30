namespace OnlineJobRecruitmentSystem.Application.DTOs.ReviewDtos
{
    public class CreateReviewDto
    {
        public int RevieweeId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
    }
}