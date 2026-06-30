using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineJobRecruitmentSystem.Application.DTOs.UserDtos;
using OnlineJobRecruitmentSystem.Application.Interfaces;
using OnlineJobRecruitmentSystem.Application.Validations.UserDtoValidation;
using OnlineJobRecruitmentSystem.Common;
using OnlineJobRecruitmentSystem.Domain.Entities;
using OnlineJobRecruitmentSystem.Infrastructure.Data;

namespace OnlineJobRecruitmentSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(
        AppDbContext context,
        IJwtService jwtService,
        IEmailService emailService) : ControllerBase
    {
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            var validator = new RegisterDtoValidation();
            var result = await validator.ValidateAsync(dto);

            if (!result.IsValid)
                return BadRequest(ResponseModel<string>.Fail(result.Errors[0].ErrorMessage));

            if (await context.Users.AnyAsync(u => u.Email == dto.Email))
                return BadRequest(ResponseModel<string>.Fail("This email already exists."));

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

            context.Users.Add(user);
            await context.SaveChangesAsync();

            await emailService.SendEmailAsync(
                dto.Email,
                "Verify your email",
                $"<h3>Welcome to NexHire!</h3>" +
                $"<p>Please verify your email by clicking the link below:</p>" +
                $"<a href='http://localhost:5179/api/auth/verify-email?token={verificationToken}'>Verify Email</a>"
            );

            return Ok(ResponseModel<string>.Ok(null!, "Registration successful. Please verify your email."));
        }

        [HttpGet("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromQuery] string token)
        {
            var user = await context.Users
                .FirstOrDefaultAsync(u => u.EmailVerificationToken == token);

            if (user == null)
                return BadRequest(ResponseModel<string>.Fail("Invalid verification token."));

            user.IsEmailVerified = true;
            user.EmailVerificationToken = null;
            await context.SaveChangesAsync();

            return Ok(ResponseModel<string>.Ok(null!, "Email verified successfully."));
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return Unauthorized(ResponseModel<string>.Fail("Invalid email or password."));

            if (!user.IsEmailVerified)
                return Unauthorized(ResponseModel<string>.Fail("Please verify your email before logging in."));

            var token = jwtService.GenerateToken(user);

            return Ok(ResponseModel<object>.Ok(new { token, role = user.Role }, "Login successful."));
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDto dto)
        {
            var user = await context.Users
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null)
                return Ok(ResponseModel<string>.Ok(null!, "If this email exists, a reset link has been sent."));

            var resetToken = Guid.NewGuid().ToString();
            user.PasswordResetToken = resetToken;
            user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1);
            await context.SaveChangesAsync();

            await emailService.SendEmailAsync(
                dto.Email,
                "Reset your password",
                $"<h3>Password Reset</h3>" +
                $"<p>Click the link below to reset your password:</p>" +
                $"<a href='http://localhost:5179/assets/pages/reset-password.html?token={resetToken}'>Reset Password</a>" +
                $"<p>This link expires in 1 hour.</p>"
            );

            return Ok(ResponseModel<string>.Ok(null!, "If this email exists, a reset link has been sent."));
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto dto)
        {
            if (dto.NewPassword != dto.ConfirmPassword)
                return BadRequest(ResponseModel<string>.Fail("Passwords do not match."));

            var user = await context.Users
                .FirstOrDefaultAsync(u => u.PasswordResetToken == dto.Token &&
                                          u.PasswordResetTokenExpiry > DateTime.UtcNow);

            if (user == null)
                return BadRequest(ResponseModel<string>.Fail("Invalid or expired reset token."));

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpiry = null;
            await context.SaveChangesAsync();

            return Ok(ResponseModel<string>.Ok(null!, "Password reset successfully."));
        }
    }
}