using OnlineJobRecruitmentSystem.Domain.Enums;

namespace OnlineJobRecruitmentSystem.Application.DTOs.ContractDtos
{
    public class ReturnContractDto
    {
        public int Id { get; set; }
        public int JobPostId { get; set; }
        public string? JobTitle { get; set; }
        public int JobSeekerProfileId { get; set; }
        public string? JobSeekerName { get; set; }
        public int JobSeekerUserId { get; set; }
        public int EmployerUserId { get; set; }
        public string? EmployerName { get; set; }
        public decimal Amount { get; set; }
        public ContractStatus Status { get; set; }
        public PaymentType PaymentType { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}