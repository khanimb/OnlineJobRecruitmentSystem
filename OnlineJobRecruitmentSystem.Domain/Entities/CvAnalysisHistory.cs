namespace OnlineJobRecruitmentSystem.Domain.Entities
{
    public class CvAnalysisHistory : BaseEntity
    {
        public int JobSeekerProfileId { get; set; }
        public int? JobPostId { get; set; }
        public int OverallScore { get; set; }
        public int? MatchScore { get; set; }
        public string ResultJson { get; set; } = string.Empty;

        public JobSeekerProfile JobSeekerProfile { get; set; } = null!;
        public JobPost? JobPost { get; set; }
    }
}
