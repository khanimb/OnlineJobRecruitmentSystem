using OnlineJobRecruitmentSystem.Domain.Entities;

namespace OnlineJobRecruitmentSystem.Domain.Entities
{
    public class User : BaseEntity
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsEmailVerified { get; set; } = false;
        public string? EmailVerificationToken { get; set; }
        public string? PasswordResetToken { get; set; }
        public DateTime? PasswordResetTokenExpiry { get; set; }

        public EmployerProfile? EmployerProfile { get; set; }
        public JobSeekerProfile? JobSeekerProfile { get; set; }
    }
}
