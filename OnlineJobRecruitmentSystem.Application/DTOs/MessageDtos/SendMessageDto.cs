namespace OnlineJobRecruitmentSystem.Application.DTOs.MessageDtos
{
    public class SendMessageDto
    {
        public int ReceiverId { get; set; }
        public string? Content { get; set; }
    }
}