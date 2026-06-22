namespace OnlineJobRecruitmentSystem.DTOs.ApplicationDtos
{
    public class ReturnApplicationDto
    {
        public int Id { get; set; }
        public int JobPostId { get; set; }
        public string JobTitle { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string CoverLetter { get; set; } = string.Empty;
        public DateTime AppliedAt { get; set; }
    }
}
