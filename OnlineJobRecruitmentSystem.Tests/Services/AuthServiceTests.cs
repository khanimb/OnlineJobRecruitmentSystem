using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using OnlineJobRecruitmentSystem.Application.DTOs.UserDtos;
using OnlineJobRecruitmentSystem.Application.Interfaces;
using OnlineJobRecruitmentSystem.Domain.Entities;
using OnlineJobRecruitmentSystem.Infrastructure.Data;
using OnlineJobRecruitmentSystem.Infrastructure.Services;

namespace OnlineJobRecruitmentSystem.Tests.Services
{
    public class AuthServiceTests
    {
        private static AppDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        private static AuthService CreateService(AppDbContext context, Mock<IEmailService>? emailMock = null, Mock<IJwtService>? jwtMock = null)
        {
            emailMock ??= new Mock<IEmailService>();
            jwtMock ??= new Mock<IJwtService>();
            var configMock = new Mock<IConfiguration>();
            configMock.Setup(c => c["App:BaseUrl"]).Returns("https://test.local");

            return new AuthService(context, jwtMock.Object, emailMock.Object, configMock.Object);
        }

        [Fact]
        public async Task RegisterAsync_DuplicateEmail_ReturnsEmailExists()
        {
            using var context = CreateContext();
            context.Users.Add(new User { Username = "existing", Email = "taken@test.com", PasswordHash = "x", Role = "JobSeeker" });
            context.SaveChanges();
            var service = CreateService(context);

            var result = await service.RegisterAsync(new RegisterDto { Username = "newuser", Email = "taken@test.com", Password = "Pass123!", Role = "JobSeeker" });

            Assert.Equal(RegisterResult.EmailExists, result);
        }

        [Fact]
        public async Task RegisterAsync_DuplicateUsername_ReturnsUsernameExists()
        {
            using var context = CreateContext();
            context.Users.Add(new User { Username = "taken", Email = "existing@test.com", PasswordHash = "x", Role = "JobSeeker" });
            context.SaveChanges();
            var service = CreateService(context);

            var result = await service.RegisterAsync(new RegisterDto { Username = "taken", Email = "new@test.com", Password = "Pass123!", Role = "JobSeeker" });

            Assert.Equal(RegisterResult.UsernameExists, result);
        }

        [Fact]
        public async Task RegisterAsync_NewUser_CreatesUnverifiedUserAndSendsEmail()
        {
            using var context = CreateContext();
            var emailMock = new Mock<IEmailService>();
            var service = CreateService(context, emailMock: emailMock);

            var result = await service.RegisterAsync(new RegisterDto { Username = "newuser", Email = "new@test.com", Password = "Pass123!", Role = "JobSeeker" });

            Assert.Equal(RegisterResult.Success, result);
            var user = await context.Users.FirstAsync(u => u.Email == "new@test.com");
            Assert.False(user.IsEmailVerified);
            emailMock.Verify(e => e.SendEmailAsync("new@test.com", It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task LoginAsync_WrongPassword_ReturnsInvalidCredentials()
        {
            using var context = CreateContext();
            context.Users.Add(new User { Username = "user", Email = "user@test.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Correct123!"), Role = "JobSeeker", IsEmailVerified = true });
            context.SaveChanges();
            var service = CreateService(context);

            var result = await service.LoginAsync(new LoginDto { Email = "user@test.com", Password = "Wrong123!" });

            Assert.Equal(LoginResult.InvalidCredentials, result);
        }

        [Fact]
        public async Task LoginAsync_EmailNotVerified_ReturnsEmailNotVerified()
        {
            using var context = CreateContext();
            context.Users.Add(new User { Username = "user", Email = "user@test.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Correct123!"), Role = "JobSeeker", IsEmailVerified = false });
            context.SaveChanges();
            var service = CreateService(context);

            var result = await service.LoginAsync(new LoginDto { Email = "user@test.com", Password = "Correct123!" });

            Assert.Equal(LoginResult.EmailNotVerified, result);
        }

        [Fact]
        public async Task LoginAsync_ValidCredentials_SendsTwoFactorCodeAndReturnsSuccess()
        {
            using var context = CreateContext();
            context.Users.Add(new User { Username = "user", Email = "user@test.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Correct123!"), Role = "JobSeeker", IsEmailVerified = true });
            context.SaveChanges();
            var emailMock = new Mock<IEmailService>();
            var service = CreateService(context, emailMock: emailMock);

            var result = await service.LoginAsync(new LoginDto { Email = "user@test.com", Password = "Correct123!" });

            Assert.Equal(LoginResult.Success, result);
            var user = await context.Users.FirstAsync(u => u.Email == "user@test.com");
            Assert.NotNull(user.TwoFactorCode);
            emailMock.Verify(e => e.SendEmailAsync("user@test.com", It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Verify2FaAsync_WrongCode_ReturnsNull()
        {
            using var context = CreateContext();
            context.Users.Add(new User { Username = "user", Email = "user@test.com", PasswordHash = "x", Role = "JobSeeker", TwoFactorCode = "111111", TwoFactorCodeExpiry = DateTime.UtcNow.AddMinutes(5) });
            context.SaveChanges();
            var service = CreateService(context);

            var result = await service.Verify2FaAsync(new Verify2FaDto { Email = "user@test.com", Code = "999999" });

            Assert.Null(result);
        }

        [Fact]
        public async Task Verify2FaAsync_ExpiredCode_ReturnsNull()
        {
            using var context = CreateContext();
            context.Users.Add(new User { Username = "user", Email = "user@test.com", PasswordHash = "x", Role = "JobSeeker", TwoFactorCode = "111111", TwoFactorCodeExpiry = DateTime.UtcNow.AddMinutes(-5) });
            context.SaveChanges();
            var service = CreateService(context);

            var result = await service.Verify2FaAsync(new Verify2FaDto { Email = "user@test.com", Code = "111111" });

            Assert.Null(result);
        }

        [Fact]
        public async Task Verify2FaAsync_ValidCode_ReturnsTokenResult()
        {
            using var context = CreateContext();
            context.Users.Add(new User { Username = "user", Email = "user@test.com", PasswordHash = "x", Role = "JobSeeker", TwoFactorCode = "111111", TwoFactorCodeExpiry = DateTime.UtcNow.AddMinutes(5) });
            context.SaveChanges();
            var jwtMock = new Mock<IJwtService>();
            jwtMock.Setup(j => j.GenerateToken(It.IsAny<User>())).Returns("fake-jwt-token");
            jwtMock.Setup(j => j.GenerateRefreshToken()).Returns("fake-refresh-token");
            var service = CreateService(context, jwtMock: jwtMock);

            var result = await service.Verify2FaAsync(new Verify2FaDto { Email = "user@test.com", Code = "111111" });

            Assert.NotNull(result);
            Assert.Equal("fake-jwt-token", result!.Token);
            Assert.Equal("JobSeeker", result.Role);
        }
    }
}