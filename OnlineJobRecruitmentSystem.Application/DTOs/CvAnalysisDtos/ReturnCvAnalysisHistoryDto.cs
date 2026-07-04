namespace OnlineJobRecruitmentSystem.Application.DTOs.CvAnalysisDtos
{
    public class ReturnCvAnalysisHistoryDto
    {
        public int Id { get; set; }
        public int? JobPostId { get; set; }
        public string? JobTitle { get; set; }
        public int OverallScore { get; set; }
        public int? MatchScore { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}