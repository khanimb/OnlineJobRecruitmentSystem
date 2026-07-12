namespace OnlineJobRecruitmentSystem.Domain.Entities
{
    public class Message : BaseEntity
    {
        public int SenderId { get; set; }
        public int ReceiverId { get; set; }
        public string Content { get; set; } = null!;
        public bool IsRead { get; set; } = false;

        public User Sender { get; set; } = null!; 
        public User Receiver { get; set; } = null!;
    }
}
