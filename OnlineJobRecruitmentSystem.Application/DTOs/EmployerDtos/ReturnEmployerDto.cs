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
}
