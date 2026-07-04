namespace OnlineJobRecruitmentSystem.Application.DTOs.CvAnalysisDtos
{
    public class JobRecommendationDto
    {
        public int JobPostId { get; set; }
        public string JobTitle { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public int MatchScore { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}
