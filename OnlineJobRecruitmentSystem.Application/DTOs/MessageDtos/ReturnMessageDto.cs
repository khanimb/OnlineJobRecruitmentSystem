namespace OnlineJobRecruitmentSystem.Application.DTOs.MessageDtos
{
    public class ReturnMessageDto
    {
        public int Id { get; set; }
        public int SenderId { get; set; }
        public string? SenderName { get; set; }
        public int ReceiverId { get; set; }
        public string? ReceiverName { get; set; }
        public string? Content { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}