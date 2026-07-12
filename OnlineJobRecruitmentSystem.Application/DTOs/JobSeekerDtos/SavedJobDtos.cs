namespace OnlineJobRecruitmentSystem.Application.DTOs.JobSeekerDtos
{
    public class SavedJobInfoDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string JobType { get; set; } = string.Empty;
        public decimal SalaryMin { get; set; }
        public decimal SalaryMax { get; set; }
        public DateTime Deadline { get; set; }
        public string CompanyName { get; set; } = string.Empty;
    }

    public class ReturnSavedJobDto
    {
        public int Id { get; set; }
        public DateTime SavedAt { get; set; }
        public SavedJobInfoDto Job { get; set; } = new();
    }
}