namespace OnlineJobRecruitmentSystem.Domain.Entities
{
    public class JobSeekerProfile : BaseEntity
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Skills { get; set; } = string.Empty;
        public string WorkExperience { get; set; } = string.Empty;
        public string CvUrl { get; set; } = string.Empty;

        public User? User { get; set; }
        public ICollection<JobApplication> Applications { get; set; } = new List<JobApplication>();
        public ICollection<SavedJob> SavedJobs { get; set; } = new List<SavedJob>();
    }
}
