namespace OnlineJobRecruitmentSystem.Application.DTOs.JobAlertDtos
{
    public class UpdateJobAlertDto
    {
        public string? Keyword { get; set; }
        public string? Location { get; set; }
        public string? Frequency { get; set; }
        public bool IsActive { get; set; }
    }
}