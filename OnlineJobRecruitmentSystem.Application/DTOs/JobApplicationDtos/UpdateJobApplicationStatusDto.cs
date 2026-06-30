using OnlineJobRecruitmentSystem.Domain.Enums;

namespace OnlineJobRecruitmentSystem.Application.DTOs.ApplicationDtos
{
    public class UpdateJobApplicationStatusDto
    {
        public ApplicationStatus Status { get; set; }
        public string Notes { get; set; } = string.Empty;
    }
}
