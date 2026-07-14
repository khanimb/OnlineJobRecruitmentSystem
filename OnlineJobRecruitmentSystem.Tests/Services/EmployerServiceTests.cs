using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using OnlineJobRecruitmentSystem.Application.DTOs.EmployerDtos;
using OnlineJobRecruitmentSystem.Application.Profiles;
using OnlineJobRecruitmentSystem.Domain.Entities;
using OnlineJobRecruitmentSystem.Domain.Enums;
using OnlineJobRecruitmentSystem.Infrastructure.Data;
using OnlineJobRecruitmentSystem.Infrastructure.Extensions;
using OnlineJobRecruitmentSystem.Infrastructure.Services;

namespace OnlineJobRecruitmentSystem.Tests.Services
{
    public class EmployerServiceTests
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
            context.JobPosts.Add(new JobPost { Id = 1, EmployerProfileId = 1, Title = "Developer", IsActive = true });
            context.JobApplications.Add(new JobApplication { Id = 1, JobPostId = 1, JobSeekerProfileId = 1, Status = ApplicationStatus.Applied });

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
        public async Task CreateProfileAsync_AlreadyExists_ReturnsNull()
        {
            using var context = CreateContext();
            var service = new EmployerService(context, CreateFileManager(), CreateMapper());

            var result = await service.CreateProfileAsync(1, new CreateEmployerDto { CompanyName = "New Co" });

            Assert.Null(result);
        }

        [Fact]
        public async Task CreateProfileAsync_NewEmployer_CreatesProfile()
        {
            using var context = CreateContext();
            var service = new EmployerService(context, CreateFileManager(), CreateMapper());

            var result = await service.CreateProfileAsync(999, new CreateEmployerDto { CompanyName = "New Co" });

            Assert.NotNull(result);
            Assert.Equal("New Co", result!.CompanyName);
        }

        [Fact]
        public async Task UpdateApplicationStatusAsync_AppliedToReviewed_Succeeds()
        {
            using var context = CreateContext();
            var service = new EmployerService(context, CreateFileManager(), CreateMapper());

            var result = await service.UpdateApplicationStatusAsync(1, 1, ApplicationStatus.Reviewed, "");

            Assert.False(result.InvalidTransition);
            Assert.Equal(ApplicationStatus.Reviewed, (await context.JobApplications.FindAsync(1))!.Status);
        }

        [Fact]
        public async Task UpdateApplicationStatusAsync_AppliedToShortlisted_InvalidTransition()
        {
            using var context = CreateContext();
            var service = new EmployerService(context, CreateFileManager(), CreateMapper());

            var result = await service.UpdateApplicationStatusAsync(1, 1, ApplicationStatus.Shortlisted, "");

            Assert.True(result.InvalidTransition);
        }

        [Fact]
        public async Task UpdateApplicationStatusAsync_ShortlistedIsTerminal_CannotChangeFurther()
        {
            using var context = CreateContext();
            var service = new EmployerService(context, CreateFileManager(), CreateMapper());
            await service.UpdateApplicationStatusAsync(1, 1, ApplicationStatus.Reviewed, "");
            await service.UpdateApplicationStatusAsync(1, 1, ApplicationStatus.Shortlisted, "");

            var result = await service.UpdateApplicationStatusAsync(1, 1, ApplicationStatus.Rejected, "");

            Assert.True(result.InvalidTransition);
        }

        [Fact]
        public async Task UpdateApplicationStatusAsync_NotOwner_ReturnsApplicationNotFound()
        {
            using var context = CreateContext();
            context.Users.Add(new User { Id = 3, Username = "other", Email = "other@test.com", PasswordHash = "x", Role = "Employer" });
            context.EmployerProfiles.Add(new EmployerProfile { Id = 2, UserId = 3, CompanyName = "Other Co" });
            context.SaveChanges();
            var service = new EmployerService(context, CreateFileManager(), CreateMapper());

            var result = await service.UpdateApplicationStatusAsync(3, 1, ApplicationStatus.Reviewed, "");

            Assert.True(result.ApplicationNotFound);
        }
    }
}