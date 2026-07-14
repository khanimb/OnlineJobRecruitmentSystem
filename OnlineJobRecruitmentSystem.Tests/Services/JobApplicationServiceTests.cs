using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OnlineJobRecruitmentSystem.Application.DTOs.JobApplicationDtos;
using OnlineJobRecruitmentSystem.Application.Interfaces;
using OnlineJobRecruitmentSystem.Application.Profiles;
using OnlineJobRecruitmentSystem.Domain.Entities;
using OnlineJobRecruitmentSystem.Infrastructure.Data;
using OnlineJobRecruitmentSystem.Infrastructure.Services;

namespace OnlineJobRecruitmentSystem.Tests.Services
{
    public class JobApplicationServiceTests
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

        private static IMapper CreateMapper()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddAutoMapper(cfg => cfg.AddProfile<MapperProfile>());
            return services.BuildServiceProvider().GetRequiredService<IMapper>();
        }

        [Fact]
        public async Task ApplyAsync_NoJobSeekerProfile_ReturnsProfileNotFound()
        {
            using var context = CreateContext();
            var service = new JobApplicationService(context, CreateMapper());

            var result = await service.ApplyAsync(999, 1, new CreateJobApplicationDto { CoverLetter = "Hi" });

            Assert.Equal(JobApplicationResult.ProfileNotFound, result);
        }

        [Fact]
        public async Task ApplyAsync_InactiveJob_ReturnsJobNotFound()
        {
            using var context = CreateContext();
            var service = new JobApplicationService(context, CreateMapper());

            var result = await service.ApplyAsync(2, 2, new CreateJobApplicationDto { CoverLetter = "Hi" });

            Assert.Equal(JobApplicationResult.JobNotFound, result);
        }

        [Fact]
        public async Task ApplyAsync_ValidApplication_ReturnsSuccess()
        {
            using var context = CreateContext();
            var service = new JobApplicationService(context, CreateMapper());

            var result = await service.ApplyAsync(2, 1, new CreateJobApplicationDto { CoverLetter = "Hi" });

            Assert.Equal(JobApplicationResult.Success, result);
            Assert.Equal(1, await context.JobApplications.CountAsync());
        }

        [Fact]
        public async Task ApplyAsync_AlreadyApplied_ReturnsAlreadyApplied()
        {
            using var context = CreateContext();
            var service = new JobApplicationService(context, CreateMapper());
            await service.ApplyAsync(2, 1, new CreateJobApplicationDto { CoverLetter = "Hi" });

            var result = await service.ApplyAsync(2, 1, new CreateJobApplicationDto { CoverLetter = "Again" });

            Assert.Equal(JobApplicationResult.AlreadyApplied, result);
        }

        [Fact]
        public async Task GetMyApplicationsAsync_NoProfile_ReturnsNull()
        {
            using var context = CreateContext();
            var service = new JobApplicationService(context, CreateMapper());

            var result = await service.GetMyApplicationsAsync(999);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetMyApplicationsAsync_WithApplications_ReturnsList()
        {
            using var context = CreateContext();
            var service = new JobApplicationService(context, CreateMapper());
            await service.ApplyAsync(2, 1, new CreateJobApplicationDto { CoverLetter = "Hi" });

            var result = await service.GetMyApplicationsAsync(2);

            Assert.NotNull(result);
            Assert.Single(result!);
        }
    }
}