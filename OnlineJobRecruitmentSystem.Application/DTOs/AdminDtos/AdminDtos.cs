namespace OnlineJobRecruitmentSystem.Application.DTOs.AdminDtos
{
    public class AdminUserDto
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsEmailVerified { get; set; }
    }

    public class AdminJobDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime Deadline { get; set; }
        public string Company { get; set; } = string.Empty;
    }

    public class AdminApplicationDto
    {
        public int Id { get; set; }
        public string JobTitle { get; set; } = string.Empty;
        public string ApplicantName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    public class AdminReviewDto
    {
        public int Id { get; set; }
        public string ReviewerEmail { get; set; } = string.Empty;
        public string RevieweeEmail { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class AdminPaymentDto
    {
        public int Id { get; set; }
        public string EmployerEmail { get; set; } = string.Empty;
        public string Plan { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class AdminStatsDto
    {
        public int TotalUsers { get; set; }
        public int TotalJobs { get; set; }
        public int ActiveJobs { get; set; }
        public int TotalApplications { get; set; }
        public int TotalPayments { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class AdminContractDto
    {
        public int Id { get; set; }
        public string JobTitle { get; set; } = string.Empty;
        public string EmployerName { get; set; } = string.Empty;
        public string JobSeekerName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}