namespace OnlineJobRecruitmentSystem.Domain.Entities
{
    public class PortfolioItem : BaseEntity
    {
        public int JobSeekerProfileId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string FileUrl { get; set; }
        public string FileType { get; set; }

        public JobSeekerProfile JobSeekerProfile { get; set; }
    }
}