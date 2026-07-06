namespace OnlineJobRecruitmentSystem.Domain.Entities
{
    public class PaymentPremium : BaseEntity
    {
        public int EmployerId { get; set; }
        public string StripePaymentId { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } 
        public string Plan { get; set; } 

        public User Employer { get; set; }
    }
}