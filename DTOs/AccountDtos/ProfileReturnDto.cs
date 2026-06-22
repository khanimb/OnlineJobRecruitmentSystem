namespace OnlineJobRecruitmentSystem.DTOs.AccountDtos
{
    public class ProfileReturnDto
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;

        public string? CompanyName { get; set; }
        public string? Description { get; set; }
        public string? Website { get; set; }
        public string? LogoUrl { get; set; }

        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public string? Skills { get; set; }
        public string? WorkExperience { get; set; }
        public string? CvUrl { get; set; }
    }
}
