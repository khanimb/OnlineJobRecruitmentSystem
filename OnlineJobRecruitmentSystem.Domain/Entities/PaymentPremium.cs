namespace OnlineJobRecruitmentSystem.Domain.Entities
{
    public class PaymentPremium : BaseEntity
    {
        public string StripePaymentId { get; set; } = null!;
        public decimal Amount { get; set; }
        public string Status { get; set; } = null!;
        public string Plan { get; set; } = null!;
        public int UserId { get; set; }

        public User Employer { get; set; } = null!;
    }
}