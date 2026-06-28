using OnlineJobRecruitmentSystem.Domain.Enums;

namespace OnlineJobRecruitmentSystem.Domain.Entities
{
    public class JobApplication : BaseEntity
    {
        public int JobPostId { get; set; }
        public int JobSeekerProfileId { get; set; }
        public ApplicationStatus Status { get; set; } = ApplicationStatus.Applied;
        public string CoverLetter { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public DateTime AppliedAt { get; set; } = DateTime.UtcNow;
        public JobPost? JobPost { get; set; }
        public JobSeekerProfile? JobSeekerProfile { get; set; }
    }
}
