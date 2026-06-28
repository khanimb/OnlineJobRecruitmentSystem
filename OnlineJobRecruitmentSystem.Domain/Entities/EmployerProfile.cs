namespace OnlineJobRecruitmentSystem.Domain.Entities
{
    public class EmployerProfile : BaseEntity
    {
        public int UserId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Website { get; set; } = string.Empty;
        public string LogoUrl { get; set; } = string.Empty;
        public User? User { get; set; }
        public ICollection<JobPost> JobPosts { get; set; } = new List<JobPost>();
    }
}
