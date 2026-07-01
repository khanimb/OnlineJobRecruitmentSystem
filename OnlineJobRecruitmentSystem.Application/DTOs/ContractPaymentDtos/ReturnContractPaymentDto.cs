using OnlineJobRecruitmentSystem.Domain.Enums;

namespace OnlineJobRecruitmentSystem.Application.DTOs.ContractPaymentDtos
{
    public class ReturnContractPaymentDto
    {
        public int Id { get; set; }
        public int ContractId { get; set; }
        public string? StripePaymentId { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PlatformFee { get; set; }
        public decimal JobSeekerAmount { get; set; }
        public ContractPaymentStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}