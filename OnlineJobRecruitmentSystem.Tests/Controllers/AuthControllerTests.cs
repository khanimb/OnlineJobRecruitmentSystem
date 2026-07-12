using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OnlineJobRecruitmentSystem.API.Controllers;
using OnlineJobRecruitmentSystem.Application.DTOs.UserDtos;
using OnlineJobRecruitmentSystem.Application.Interfaces;
using OnlineJobRecruitmentSystem.Application.Validations.UserDtoValidation;
using OnlineJobRecruitmentSystem.Domain.Entities;
using OnlineJobRecruitmentSystem.Infrastructure.Data;
using OnlineJobRecruitmentSystem.Infrastructure.Services;
using System.Security.Claims;

namespace OnlineJobRecruitmentSystem.Tests.Controllers
{
    public class FakeEmailService : IEmailService
    {
        public Task SendEmailAsync(string to, string subject, string body) => Task.CompletedTask;
    }

    public class AuthControllerTests
    {
        private static AppDbContext CreateContext() =>
            new(new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);

        private static IConfiguration CreateConfiguration()
        {
            var configValues = new Dictionary<string, string?>
            {
                { "Jwt:Key", "TestSecretKeyForUnitTestsOnly12345!" },
                { "Jwt:Issuer", "TestIssuer" },
                { "Jwt:Audience", "TestAudience" },
                { "App:BaseUrl", "http://localhost:5179" }
            };
            return new ConfigurationBuilder().AddInMemoryCollection(configValues).Build();
        }

        private static AuthController CreateController(AppDbContext context)
        {
            var configuration = CreateConfiguration();
            var jwtService = new JwtService(configuration);
            var authService = new AuthService(context, jwtService, new FakeEmailService(), configuration);

            var controller = new AuthController(
                authService,
                new RegisterDtoValidation(),
                new LoginDtoValidation(),
                new ForgotPasswordDtoValidation(),
                new ResetPasswordDtoValidation(),
                new RefreshTokenDtoValidation(),
                new Verify2FaDtoValidation());

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity()) }
            };

            return controller;
        }

        [Fact]
        public async Task Register_NewUser_CreatesUserAndReturnsOk()
        {
            using var context = CreateContext();
            var controller = CreateController(context);
            var dto = new RegisterDto { Username = "newuser", Email = "new@test.com", Password = "Test1234", Role = "JobSeeker" };

            var result = await controller.Register(dto);

            Assert.IsType<OkObjectResult>(result);
            Assert.Equal(1, context.Users.Count());
            Assert.False(context.Users.First().IsEmailVerified);
        }

        [Fact]
        public async Task Register_DuplicateEmail_ReturnsBadRequest()
        {
            using var context = CreateContext();
            context.Users.Add(new User { Username = "existing", Email = "dup@test.com", PasswordHash = "x", Role = "JobSeeker" });
            context.SaveChanges();
            var controller = CreateController(context);
            var dto = new RegisterDto { Username = "newuser", Email = "dup@test.com", Password = "Test1234", Role = "JobSeeker" };

            var result = await controller.Register(dto);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Register_DuplicateUsername_ReturnsBadRequest()
        {
            using var context = CreateContext();
            context.Users.Add(new User { Username = "dupname", Email = "a@test.com", PasswordHash = "x", Role = "JobSeeker" });
            context.SaveChanges();
            var controller = CreateController(context);
            var dto = new RegisterDto { Username = "dupname", Email = "new@test.com", Password = "Test1234", Role = "JobSeeker" };

            var result = await controller.Register(dto);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Login_WrongPassword_ReturnsUnauthorized()
        {
            using var context = CreateContext();
            context.Users.Add(new User
            {
                Username = "user1",
                Email = "user1@test.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPass1"),
                Role = "JobSeeker",
                IsEmailVerified = true
            });
            context.SaveChanges();
            var controller = CreateController(context);
            var dto = new LoginDto { Email = "user1@test.com", Password = "WrongPassword" };

            var result = await controller.Login(dto);

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task Login_UnverifiedEmail_ReturnsUnauthorized()
        {
            using var context = CreateContext();
            context.Users.Add(new User
            {
                Username = "user1",
                Email = "user1@test.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test1234"),
                Role = "JobSeeker",
                IsEmailVerified = false
            });
            context.SaveChanges();
            var controller = CreateController(context);
            var dto = new LoginDto { Email = "user1@test.com", Password = "Test1234" };

            var result = await controller.Login(dto);

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task Login_ValidCredentials_GeneratesTwoFactorCode()
        {
            using var context = CreateContext();
            context.Users.Add(new User
            {
                Username = "user1",
                Email = "user1@test.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test1234"),
                Role = "JobSeeker",
                IsEmailVerified = true
            });
            context.SaveChanges();
            var controller = CreateController(context);
            var dto = new LoginDto { Email = "user1@test.com", Password = "Test1234" };

            var result = await controller.Login(dto);

            Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(context.Users.First().TwoFactorCode);
        }

        [Fact]
        public async Task Verify2Fa_InvalidCode_ReturnsUnauthorized()
        {
            using var context = CreateContext();
            context.Users.Add(new User
            {
                Username = "user1",
                Email = "user1@test.com",
                PasswordHash = "x",
                Role = "JobSeeker",
                TwoFactorCode = "111111",
                TwoFactorCodeExpiry = DateTime.UtcNow.AddMinutes(10)
            });
            context.SaveChanges();
            var controller = CreateController(context);
            var dto = new Verify2FaDto { Email = "user1@test.com", Code = "999999" };

            var result = await controller.Verify2Fa(dto);

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task Verify2Fa_ValidCode_ReturnsTokenAndClearsCode()
        {
            using var context = CreateContext();
            context.Users.Add(new User
            {
                Username = "user1",
                Email = "user1@test.com",
                PasswordHash = "x",
                Role = "JobSeeker",
                TwoFactorCode = "123456",
                TwoFactorCodeExpiry = DateTime.UtcNow.AddMinutes(10)
            });
            context.SaveChanges();
            var controller = CreateController(context);
            var dto = new Verify2FaDto { Email = "user1@test.com", Code = "123456" };

            var result = await controller.Verify2Fa(dto);

            Assert.IsType<OkObjectResult>(result);
            Assert.Null(context.Users.First().TwoFactorCode);
        }
    }
}