namespace OnlineJobRecruitmentSystem.Models
{
    public class Application : BaseEntity
    {
        public int JobPostId { get; set; }
        public int JobSeekerProfileId { get; set; }
        public string Status { get; set; } = "Applied"; 
        public string CoverLetter { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public DateTime AppliedAt { get; set; } = DateTime.UtcNow;
        public JobPost? JobPost { get; set; }
        public JobSeekerProfile? JobSeekerProfile { get; set; }
    }
}
