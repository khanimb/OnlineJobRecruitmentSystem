namespace OnlineJobRecruitmentSystem.Domain.Entities
{
    public class Comment : BaseEntity
    {
        public int JobPostId { get; set; }
        public int UserId { get; set; }
        public string Text { get; set; } = string.Empty;

        public JobPost JobPost { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}
