namespace OnlineJobRecruitmentSystem.Domain.Entities
{
    public class JobAlert : BaseEntity
    {
        public int UserId { get; set; }
        public string Keyword { get; set; }
        public string Location { get; set; }
        public string Frequency { get; set; } 
        public bool IsActive { get; set; } = true;

        public User User { get; set; }
    }
}