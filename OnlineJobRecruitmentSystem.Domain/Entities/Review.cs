namespace OnlineJobRecruitmentSystem.Domain.Entities
{
    public class Review : BaseEntity
    {
        public int ReviewerId { get; set; }
        public int RevieweeId { get; set; }
        public int Rating { get; set; } 
        public string Comment { get; set; } = null!;

        public User Reviewer { get; set; } = null!;
        public User Reviewee { get; set; } = null!;
    }
}
