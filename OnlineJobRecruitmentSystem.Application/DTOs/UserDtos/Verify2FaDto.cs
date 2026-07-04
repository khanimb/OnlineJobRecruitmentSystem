namespace OnlineJobRecruitmentSystem.Application.DTOs.UserDtos
{
    public class Verify2FaDto
    {
        public string Email { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }
}