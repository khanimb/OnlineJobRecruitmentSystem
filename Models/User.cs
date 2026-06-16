namespace OnlineJobRecruitmentSystem.Models
{
    public class User : BaseEntity
    {
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;

        public EmployerProfile? EmployerProfile { get; set; }
        public JobSeekerProfile? JobSeekerProfile { get; set; }
    }
}
