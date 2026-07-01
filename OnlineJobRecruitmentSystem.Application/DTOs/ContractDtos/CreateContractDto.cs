namespace OnlineJobRecruitmentSystem.Application.DTOs.ContractDtos
{
    public class CreateContractDto
    {
        public int JobPostId { get; set; }
        public int JobSeekerProfileId { get; set; }
        public decimal Amount { get; set; }
    }
}
