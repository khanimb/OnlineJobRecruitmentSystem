namespace OnlineJobRecruitmentSystem.Application.DTOs.PaymentDtos
{
    public class ReturnPaymentDto
    {
        public int Id { get; set; }
        public string? Plan { get; set; }
        public decimal Amount { get; set; }
        public string? Status { get; set; }
        public string? StripePaymentId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}