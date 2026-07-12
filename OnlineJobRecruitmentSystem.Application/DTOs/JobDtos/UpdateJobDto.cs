namespace OnlineJobRecruitmentSystem.Application.DTOs.JobDtos
{
    public class UpdateJobDto
    {
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
        public decimal? Budget { get; set; }
        public string? PaymentType { get; set; }
    }
}
