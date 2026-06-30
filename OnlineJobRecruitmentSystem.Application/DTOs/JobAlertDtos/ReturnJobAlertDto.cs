namespace OnlineJobRecruitmentSystem.Application.DTOs.JobAlertDtos
{
    public class ReturnJobAlertDto
    {
        public int Id { get; set; }
        public string? Keyword { get; set; }
        public string? Location { get; set; }
        public string? Frequency { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}