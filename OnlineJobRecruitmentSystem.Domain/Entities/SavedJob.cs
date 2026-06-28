using OnlineJobRecruitmentSystem.Domain.Entities;

namespace OnlineJobRecruitmentSystem.Domain.Entities
{
    public class SavedJob : BaseEntity
    {
        public int JobSeekerProfileId { get; set; }
        public int JobPostId { get; set; }
        public DateTime SavedAt { get; set; } = DateTime.UtcNow;
        public JobSeekerProfile? JobSeekerProfile { get; set; }
        public JobPost? JobPost { get; set; }
    }
}
