using OnlineJobRecruitmentSystem.Domain.Enums;

namespace OnlineJobRecruitmentSystem.Application.DTOs.EmployerDtos
{
    public class ReturnEmployerDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Website { get; set; } = string.Empty;
        public string LogoUrl { get; set; } = string.Empty;
    }

    public class ReturnEmployerApplicantDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Skills { get; set; } = string.Empty;
        public string CvUrl { get; set; } = string.Empty;
    }

    public class ReturnEmployerApplicationDto
    {
        public int Id { get; set; }
        public ApplicationStatus Status { get; set; }
        public string CoverLetter { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public DateTime AppliedAt { get; set; }
        public int JobPostId { get; set; }
        public string JobTitle { get; set; } = string.Empty;
        public ReturnEmployerApplicantDto JobSeeker { get; set; } = new();
    }

    public class JobApplicantSummaryDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Skills { get; set; } = string.Empty;
        public string CvUrl { get; set; } = string.Empty;
    }

    public class ReturnJobApplicantDto
    {
        public int Id { get; set; }
        public ApplicationStatus Status { get; set; }
        public string CoverLetter { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public DateTime AppliedAt { get; set; }
        public JobApplicantSummaryDto JobSeeker { get; set; } = new();
    }

    public class EmployerDashboardDto
    {
        public int TotalJobs { get; set; }
        public int TotalApplications { get; set; }
        public int Shortlisted { get; set; }
        public int Rejected { get; set; }
    }
}