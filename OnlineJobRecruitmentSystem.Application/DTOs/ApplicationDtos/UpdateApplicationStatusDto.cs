using OnlineJobRecruitmentSystem.Domain.Enums;

namespace OnlineJobRecruitmentSystem.Application.DTOs.ApplicationDtos
{
    public class UpdateApplicationStatusDto
    {
        public ApplicationStatus Status { get; set; }
        public string Notes { get; set; } = string.Empty;
    }
}
