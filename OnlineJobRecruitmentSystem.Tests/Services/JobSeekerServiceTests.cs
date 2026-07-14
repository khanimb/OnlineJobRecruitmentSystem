using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using OnlineJobRecruitmentSystem.Application.Interfaces;
using OnlineJobRecruitmentSystem.Application.Profiles;
using OnlineJobRecruitmentSystem.Domain.Entities;
using OnlineJobRecruitmentSystem.Infrastructure.Data;
using OnlineJobRecruitmentSystem.Infrastructure.Extensions;
using OnlineJobRecruitmentSystem.Infrastructure.Services;

namespace OnlineJobRecruitmentSystem.Tests.Services
{
    public class JobSeekerServiceTests
    {
        private static AppDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var context = new AppDbContext(options);

            context.Users.AddRange(
                new User { Id = 1, Username = "employer", Email = "employer@test.com", PasswordHash = "x", Role = "Employer" },
                new User { Id = 2, Username = "seeker", Email = "seeker@test.com", PasswordHash = "x", Role = "JobSeeker" }
            );
            context.EmployerProfiles.Add(new EmployerProfile { Id = 1, UserId = 1, CompanyName = "Acme" });
            context.JobSeekerProfiles.Add(new JobSeekerProfile { Id = 1, UserId = 2, FullName = "Jane Doe" });
            context.JobPosts.AddRange(
                new JobPost { Id = 1, EmployerProfileId = 1, Title = "Developer", IsActive = true },
                new JobPost { Id = 2, EmployerProfileId = 1, Title = "Old Job", IsActive = false }
            );

            context.SaveChanges();
            return context;
        }

        private static FileManager CreateFileManager()
        {
            var envMock = new Mock<IWebHostEnvironment>();
            envMock.Setup(e => e.WebRootPath).Returns(Path.GetTempPath());
            return new FileManager(envMock.Object);
        }

        private static IMapper CreateMapper()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddAutoMapper(cfg => cfg.AddProfile<MapperProfile>());
            return services.BuildServiceProvider().GetRequiredService<IMapper>();
        }

        [Fact]
        public async Task SaveJobAsync_NoProfile_ReturnsProfileNotFound()
        {
            using var context = CreateContext();
            var service = new JobSeekerService(context, CreateFileManager(), CreateMapper());

            var result = await service.SaveJobAsync(999, 1);

            Assert.Equal(SaveJobResult.ProfileNotFound, result);
        }

        [Fact]
        public async Task SaveJobAsync_InactiveJob_ReturnsJobNotFound()
        {
            using var context = CreateContext();
            var service = new JobSeekerService(context, CreateFileManager(), CreateMapper());

            var result = await service.SaveJobAsync(2, 2);

            Assert.Equal(SaveJobResult.JobNotFound, result);
        }

        [Fact]
        public async Task SaveJobAsync_ValidJob_ReturnsSuccess()
        {
            using var context = CreateContext();
            var service = new JobSeekerService(context, CreateFileManager(), CreateMapper());

            var result = await service.SaveJobAsync(2, 1);

            Assert.Equal(SaveJobResult.Success, result);
            Assert.Equal(1, await context.SavedJobs.CountAsync());
        }

        [Fact]
        public async Task SaveJobAsync_AlreadySaved_ReturnsAlreadySaved()
        {
            using var context = CreateContext();
            var service = new JobSeekerService(context, CreateFileManager(), CreateMapper());
            await service.SaveJobAsync(2, 1);

            var result = await service.SaveJobAsync(2, 1);

            Assert.Equal(SaveJobResult.AlreadySaved, result);
        }

        [Fact]
        public async Task RemoveSavedJobAsync_NotSaved_ReturnsSavedJobNotFound()
        {
            using var context = CreateContext();
            var service = new JobSeekerService(context, CreateFileManager(), CreateMapper());

            var result = await service.RemoveSavedJobAsync(2, 1);

            Assert.Equal(RemoveSavedJobResult.SavedJobNotFound, result);
        }

        [Fact]
        public async Task RemoveSavedJobAsync_ValidRemoval_ReturnsSuccess()
        {
            using var context = CreateContext();
            var service = new JobSeekerService(context, CreateFileManager(), CreateMapper());
            await service.SaveJobAsync(2, 1);

            var result = await service.RemoveSavedJobAsync(2, 1);

            Assert.Equal(RemoveSavedJobResult.Success, result);
            Assert.Equal(0, await context.SavedJobs.CountAsync());
        }
    }
}