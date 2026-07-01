using OnlineJobRecruitmentSystem.Domain.Enums;

namespace OnlineJobRecruitmentSystem.Domain.Entities
{
    public class ContractPayment : BaseEntity
    {
        public int ContractId { get; set; }
        public string StripePaymentId { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public decimal PlatformFee { get; set; }
        public decimal JobSeekerAmount { get; set; }
        public ContractPaymentStatus Status { get; set; } = ContractPaymentStatus.Pending;

        public Contract Contract { get; set; }
    }
}
