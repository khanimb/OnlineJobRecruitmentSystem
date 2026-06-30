namespace OnlineJobRecruitmentSystem.Application.DTOs.AnalyticsDtos
{
    public class EmployerStatsDto
    {
        public int TotalJobs { get; set; }
        public int TotalApplications { get; set; }
        public int Pending { get; set; }
        public int Shortlisted { get; set; }
        public int Rejected { get; set; }
        public List<ApplicationTrendDto> ApplicationTrend { get; set; } = new();
    }
}