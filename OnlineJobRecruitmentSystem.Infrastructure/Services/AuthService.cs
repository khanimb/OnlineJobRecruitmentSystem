using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OnlineJobRecruitmentSystem.Application.DTOs.UserDtos;
using OnlineJobRecruitmentSystem.Application.Interfaces;
using OnlineJobRecruitmentSystem.Domain.Entities;
using OnlineJobRecruitmentSystem.Infrastructure.Data;

namespace OnlineJobRecruitmentSystem.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IJwtService _jwtService;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;

        public AuthService(AppDbContext context, IJwtService jwtService, IEmailService emailService, IConfiguration configuration)
        {
            _context = context;
            _jwtService = jwtService;
            _emailService = emailService;
            _configuration = configuration;
        }

        public async Task<RegisterResult> RegisterAsync(RegisterDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                return RegisterResult.EmailExists;

            if (await _context.Users.AnyAsync(u => u.Username == dto.Username))
                return RegisterResult.UsernameExists;

            var verificationToken = Guid.NewGuid().ToString();

            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = dto.Role,
                IsEmailVerified = false,
                EmailVerificationToken = verificationToken
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var baseUrl = _configuration["App:BaseUrl"];
            await _emailService.SendEmailAsync(
                dto.Email,
                "Verify your email",
                $"<h3>Welcome to NexHire!</h3>" +
                $"<p>Please verify your email by clicking the link below:</p>" +
                $"<a href='{baseUrl}/assets/pages/verifyemail.html?token={verificationToken}'>Verify Email</a>"
            );

            return RegisterResult.Success;
        }

        public async Task<bool> VerifyEmailAsync(string token)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.EmailVerificationToken == token);

            if (user == null) return false;

            user.IsEmailVerified = true;
            user.EmailVerificationToken = null;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<LoginResult> LoginAsync(LoginDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return LoginResult.InvalidCredentials;

            if (!user.IsEmailVerified)
                return LoginResult.EmailNotVerified;

            var code = Random.Shared.Next(100000, 999999).ToString();

            user.TwoFactorCode = code;
            user.TwoFactorCodeExpiry = DateTime.UtcNow.AddMinutes(10);
            await _context.SaveChangesAsync();

            await _emailService.SendEmailAsync(
                user.Email,
                "Your verification code",
                $"<h3>Your login verification code is: <b>{code}</b></h3><p>This code expires in 10 minutes.</p>"
            );

            return LoginResult.Success;
        }

        public async Task ForgotPasswordAsync(ForgotPasswordDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null) return;

            var resetToken = Guid.NewGuid().ToString();
            user.PasswordResetToken = resetToken;
            user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1);
            await _context.SaveChangesAsync();

            var baseUrl = _configuration["App:BaseUrl"];
            await _emailService.SendEmailAsync(
                dto.Email,
                "Reset your password",
                $"<h3>Password Reset</h3>" +
                $"<p>Click the link below to reset your password:</p>" +
                $"<a href='{baseUrl}/assets/pages/resetpassword.html?token={resetToken}'>Reset Password</a>" +
                $"<p>This link expires in 1 hour.</p>"
            );
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordDto dto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.PasswordResetToken == dto.Token &&
                                          u.PasswordResetTokenExpiry > DateTime.UtcNow);

            if (user == null) return false;

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpiry = null;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<TokenResult?> RefreshTokenAsync(RefreshTokenDto dto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.RefreshToken == dto.RefreshToken);

            if (user == null || user.RefreshTokenExpiry == null || user.RefreshTokenExpiry <= DateTime.UtcNow)
                return null;

            var newToken = _jwtService.GenerateToken(user);
            var newRefreshToken = _jwtService.GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(30);
            await _context.SaveChangesAsync();

            return new TokenResult { Id = user.Id, Token = newToken, RefreshToken = newRefreshToken };
        }

        public async Task LogoutAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);

            if (user != null)
            {
                user.RefreshToken = null;
                user.RefreshTokenExpiry = null;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<TokenResult?> Verify2FaAsync(Verify2FaDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null || user.TwoFactorCode != dto.Code || user.TwoFactorCodeExpiry == null || user.TwoFactorCodeExpiry <= DateTime.UtcNow)
                return null;

            user.TwoFactorCode = null;
            user.TwoFactorCodeExpiry = null;

            var token = _jwtService.GenerateToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(30);
            await _context.SaveChangesAsync();

            return new TokenResult { Id = user.Id, Token = token, RefreshToken = refreshToken, Role = user.Role };
        }
    }
}