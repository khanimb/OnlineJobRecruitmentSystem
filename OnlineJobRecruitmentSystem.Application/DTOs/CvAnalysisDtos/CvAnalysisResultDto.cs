namespace OnlineJobRecruitmentSystem.Application.DTOs.CvAnalysisDtos
{
    public class CvAnalysisResultDto
    {
        public int OverallScore { get; set; }
        public List<string> Strengths { get; set; } = new();
        public List<string> Weaknesses { get; set; } = new();
        public List<string> Suggestions { get; set; } = new();
        public int? MatchScore { get; set; }
        public List<string>? MissingSkills { get; set; }
    }
}
