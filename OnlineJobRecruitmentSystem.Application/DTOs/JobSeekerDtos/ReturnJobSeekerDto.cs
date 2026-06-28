namespace OnlineJobRecruitmentSystem.Application.DTOs.JobSeekerDtos
{
    public class ReturnJobSeekerDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Skills { get; set; } = string.Empty;
        public string WorkExperience { get; set; } = string.Empty;
        public string CvUrl { get; set; } = string.Empty;
    }
}
