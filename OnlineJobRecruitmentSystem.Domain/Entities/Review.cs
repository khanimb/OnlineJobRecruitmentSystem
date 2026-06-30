namespace OnlineJobRecruitmentSystem.Domain.Entities
{
    public class Review : BaseEntity
    {
        public int ReviewerId { get; set; }
        public int RevieweeId { get; set; }
        public int Rating { get; set; } 
        public string Comment { get; set; }

        public User Reviewer { get; set; }
        public User Reviewee { get; set; }
    }
}
