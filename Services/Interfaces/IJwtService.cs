using OnlineJobRecruitmentSystem.Models;

namespace OnlineJobRecruitmentSystem.Services.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(User user);
    }
}
