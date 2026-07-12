namespace OnlineJobRecruitmentSystem.Domain.Entities
{
    public class PortfolioItem : BaseEntity
    {
        public int JobSeekerProfileId { get; set; }
        public string Title { get; set; } = null!; 
        public string Description { get; set; } = null!;
        public string FileUrl { get; set; } = null!;
        public string FileType { get; set; } = null!;

        public JobSeekerProfile JobSeekerProfile { get; set; } = null!;
    }
}