namespace OnlineJobRecruitmentSystem.Application.DTOs.JobDtos
{
    public class PlatformStatsDto
    {
        public int TotalActiveJobs { get; set; }
        public int TotalCompanies { get; set; }
        public int TotalJobSeekers { get; set; }
        public double SuccessRate { get; set; }
    }
}