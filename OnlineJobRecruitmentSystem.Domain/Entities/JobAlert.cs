namespace OnlineJobRecruitmentSystem.Domain.Entities
{
    public class JobAlert : BaseEntity
    {
        public int UserId { get; set; }
        public string Keyword { get; set; } = null!;
        public string Location { get; set; } = null!;  
        public string Frequency { get; set; } = null!;
        public bool IsActive { get; set; } = true;

        public User User { get; set; } = null!;
    }
}