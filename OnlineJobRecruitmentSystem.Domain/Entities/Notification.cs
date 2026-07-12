namespace OnlineJobRecruitmentSystem.Domain.Entities
{
    public class Notification : BaseEntity
    {
        public int UserId { get; set; }
        public string Title { get; set; } = null!;
        public string Message { get; set; } = null!;
        public bool IsRead { get; set; } = false;
        public string Type { get; set; } = null!;

        public User User { get; set; } = null!;
    }
}
