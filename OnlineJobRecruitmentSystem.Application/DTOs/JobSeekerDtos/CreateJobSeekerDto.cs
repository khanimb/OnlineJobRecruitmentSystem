namespace OnlineJobRecruitmentSystem.Application.DTOs.JobSeekerDtos
{
    public class CreateJobSeekerDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Skills { get; set; } = string.Empty;
        public string WorkExperience { get; set; } = string.Empty;
    }
}
