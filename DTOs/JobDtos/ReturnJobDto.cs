namespace OnlineJobRecruitmentSystem.DTOs.JobDtos
{
    public class ReturnJobDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Requirements { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string JobType { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal SalaryMin { get; set; }
        public decimal SalaryMax { get; set; }
        public DateTime Deadline { get; set; }
        public bool IsActive { get; set; }
        public string CompanyName { get; set; } = string.Empty;
    }
}