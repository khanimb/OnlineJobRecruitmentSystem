using OnlineJobRecruitmentSystem.Domain.Enums;

namespace OnlineJobRecruitmentSystem.Domain.Entities
{
    public class Contract : BaseEntity
    {
        public int JobPostId { get; set; }
        public int EmployerProfileId { get; set; }
        public int JobSeekerProfileId { get; set; }
        public ContractStatus Status { get; set; } = ContractStatus.Active;
        public decimal Amount { get; set; }
        public PaymentType PaymentType { get; set; } = PaymentType.Fixed;
        public DateTime? CompletedAt { get; set; }

        public JobPost JobPost { get; set; }
        public EmployerProfile EmployerProfile { get; set; }
        public JobSeekerProfile JobSeekerProfile { get; set; }
    }
}