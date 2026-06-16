using static System.Net.Mime.MediaTypeNames;

namespace OnlineJobRecruitmentSystem.Models
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
        public ICollection<Application> Applications { get; set; } = new List<Application>();
        public ICollection<SavedJob> SavedJobs { get; set; } = new List<SavedJob>();
    }
}
