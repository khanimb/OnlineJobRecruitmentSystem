using OnlineJobRecruitmentSystem.Application.DTOs.UserDtos;

namespace OnlineJobRecruitmentSystem.Application.Interfaces
{

    public class TokenResult
    {
        public int Id { get; set; }
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public string? Role { get; set; }
    }

    public interface IAuthService
    {
        Task<RegisterResult> RegisterAsync(RegisterDto dto);
        Task<bool> VerifyEmailAsync(string token);
        Task<LoginResult> LoginAsync(LoginDto dto);
        Task ForgotPasswordAsync(ForgotPasswordDto dto);
        Task<bool> ResetPasswordAsync(ResetPasswordDto dto);
        Task<TokenResult?> RefreshTokenAsync(RefreshTokenDto dto);
        Task LogoutAsync(int userId);
        Task<TokenResult?> Verify2FaAsync(Verify2FaDto dto);
    }
}