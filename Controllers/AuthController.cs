using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineJobRecruitmentSystem.Data;
using OnlineJobRecruitmentSystem.DTOs;
using OnlineJobRecruitmentSystem.Models;
using OnlineJobRecruitmentSystem.Services.Interfaces;
using OnlineJobRecruitmentSystem.Validations.UserDtoValidation;

namespace OnlineJobRecruitmentSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IJwtService _jwtService;

        public AuthController(AppDbContext context, IJwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            var validator = new RegisterDtoValidation();
            var result = await validator.ValidateAsync(dto);

            if (!result.IsValid)
                return BadRequest(ResponseModel<string>.Fail(result.Errors[0].ErrorMessage));

            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                return BadRequest(ResponseModel<string>.Fail("This email already exists."));

            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = dto.Role
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(ResponseModel<string>.Ok(null!, "Registration successful."));
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return Unauthorized(ResponseModel<string>.Fail("Invalid email or password."));

            var token = _jwtService.GenerateToken(user);

            return Ok(ResponseModel<object>.Ok(new { token, role = user.Role }, "Login successful."));
        }
    }
}
