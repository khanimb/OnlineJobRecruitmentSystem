using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineJobRecruitmentSystem.Application.DTOs.UserDtos;
using OnlineJobRecruitmentSystem.Application.Interfaces;
using OnlineJobRecruitmentSystem.Common;

namespace OnlineJobRecruitmentSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(
        IAuthService authService,
        IValidator<RegisterDto> registerValidator,
        IValidator<LoginDto> loginValidator,
        IValidator<ForgotPasswordDto> forgotPasswordValidator,
        IValidator<ResetPasswordDto> resetPasswordValidator,
        IValidator<RefreshTokenDto> refreshTokenValidator,
        IValidator<Verify2FaDto> verify2FaValidator) : BaseApiController
    {
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            var result = await registerValidator.ValidateAsync(dto);
            if (!result.IsValid)
                return BadRequest(ResponseModel<string>.Fail(result.Errors[0].ErrorMessage));

            var registerResult = await authService.RegisterAsync(dto);

            return registerResult switch
            {
                RegisterResult.EmailExists => BadRequest(ResponseModel<string>.Fail("This email already exists.")),
                RegisterResult.UsernameExists => BadRequest(ResponseModel<string>.Fail("This username already exists.")),
                _ => Ok(ResponseModel<string>.Ok(null!, "Registration successful. Please verify your email."))
            };
        }

        [HttpGet("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromQuery] VerifyEmailDto dto)
        {
            var verified = await authService.VerifyEmailAsync(dto.Token);
            if (!verified)
                return BadRequest(ResponseModel<string>.Fail("Invalid verification token."));

            return Ok(ResponseModel<string>.Ok(null!, "Email verified successfully."));
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var result = await loginValidator.ValidateAsync(dto);
            if (!result.IsValid)
                return BadRequest(ResponseModel<string>.Fail(result.Errors[0].ErrorMessage));

            var loginResult = await authService.LoginAsync(dto);

            return loginResult switch
            {
                LoginResult.InvalidCredentials => Unauthorized(ResponseModel<string>.Fail("Invalid email or password.")),
                LoginResult.EmailNotVerified => Unauthorized(ResponseModel<string>.Fail("Please verify your email before logging in.")),
                _ => Ok(ResponseModel<string>.Ok(null!, "Verification code sent to your email."))
            };
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDto dto)
        {
            var result = await forgotPasswordValidator.ValidateAsync(dto);
            if (!result.IsValid)
                return BadRequest(ResponseModel<string>.Fail(result.Errors[0].ErrorMessage));

            await authService.ForgotPasswordAsync(dto);

            return Ok(ResponseModel<string>.Ok(null!, "If this email exists, a reset link has been sent."));
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto dto)
        {
            var result = await resetPasswordValidator.ValidateAsync(dto);
            if (!result.IsValid)
                return BadRequest(ResponseModel<string>.Fail(result.Errors[0].ErrorMessage));

            var reset = await authService.ResetPasswordAsync(dto);
            if (!reset)
                return BadRequest(ResponseModel<string>.Fail("Invalid or expired reset token."));

            return Ok(ResponseModel<string>.Ok(null!, "Password reset successfully."));
        }

        [HttpPost("refresh-token")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshToken(RefreshTokenDto dto)
        {
            var result = await refreshTokenValidator.ValidateAsync(dto);
            if (!result.IsValid)
                return BadRequest(ResponseModel<string>.Fail(result.Errors[0].ErrorMessage));

            var tokenResult = await authService.RefreshTokenAsync(dto);
            if (tokenResult == null)
                return Unauthorized(ResponseModel<string>.Fail("Invalid or expired refresh token."));

            return Ok(ResponseModel<object>.Ok(new { token = tokenResult.Token, refreshToken = tokenResult.RefreshToken }, "Token refreshed."));
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await authService.LogoutAsync(CurrentUserId);
            return Ok(ResponseModel<string>.Ok(null!, "Logged out."));
        }

        [HttpPost("verify-2fa")]
        [AllowAnonymous]
        public async Task<IActionResult> Verify2Fa(Verify2FaDto dto)
        {
            var result = await verify2FaValidator.ValidateAsync(dto);
            if (!result.IsValid)
                return BadRequest(ResponseModel<string>.Fail(result.Errors[0].ErrorMessage));

            var tokenResult = await authService.Verify2FaAsync(dto);
            if (tokenResult == null)
                return Unauthorized(ResponseModel<string>.Fail("Invalid or expired verification code."));

            return Ok(ResponseModel<object>.Ok(new { token = tokenResult.Token, refreshToken = tokenResult.RefreshToken, role = tokenResult.Role }, "Login successful."));
        }
    }
}