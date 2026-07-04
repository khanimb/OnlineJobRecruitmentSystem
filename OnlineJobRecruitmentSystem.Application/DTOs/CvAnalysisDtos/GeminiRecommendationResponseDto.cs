namespace OnlineJobRecruitmentSystem.Application.DTOs.CvAnalysisDtos
{
    public class GeminiRecommendationResponseDto
    {
        public List<GeminiRecommendationItemDto> Recommendations { get; set; } = new();
    }

    public class GeminiRecommendationItemDto
    {
        public int JobPostId { get; set; }
        public int MatchScore { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}