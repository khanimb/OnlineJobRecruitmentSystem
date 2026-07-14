using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OnlineJobRecruitmentSystem.Application.DTOs.JobDtos;
using OnlineJobRecruitmentSystem.Application.Interfaces;
using OnlineJobRecruitmentSystem.Application.Profiles;
using OnlineJobRecruitmentSystem.Domain.Entities;
using OnlineJobRecruitmentSystem.Infrastructure.Data;
using OnlineJobRecruitmentSystem.Infrastructure.Services;

namespace OnlineJobRecruitmentSystem.Tests.Services
{
    public class JobServiceTests
    {
        private static AppDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var context = new AppDbContext(options);

            context.Users.Add(new User { Id = 1, Username = "employer", Email = "employer@test.com", PasswordHash = "x", Role = "Employer" });
            context.EmployerProfiles.Add(new EmployerProfile { Id = 1, UserId = 1, CompanyName = "Acme" });
            context.JobPosts.AddRange(
                new JobPost { Id = 1, EmployerProfileId = 1, Title = "Backend Dev", Category = "Technology", Location = "Baku", JobType = "FullTime", SalaryMin = 2000, SalaryMax = 3000, IsActive = true },
                new JobPost { Id = 2, EmployerProfileId = 1, Title = "Marketing Lead", Category = "Marketing", Location = "Remote", JobType = "Remote", SalaryMin = 1000, SalaryMax = 1500, IsActive = true },
                new JobPost { Id = 3, EmployerProfileId = 1, Title = "Inactive Job", Category = "Technology", Location = "Baku", JobType = "FullTime", SalaryMin = 5000, SalaryMax = 6000, IsActive = false }
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
        public async Task GetJobsAsync_NoFilters_ExcludesInactiveJobs()
        {
            using var context = CreateContext();
            var service = new JobService(context, CreateMapper());

            var result = await service.GetJobsAsync(null, null, null, null, null, 1, 10);

            Assert.Equal(2, result.TotalCount);
        }

        [Fact]
        public async Task GetJobsAsync_FilterByCategory_ReturnsMatchingOnly()
        {
            using var context = CreateContext();
            var service = new JobService(context, CreateMapper());

            var result = await service.GetJobsAsync("Marketing", null, null, null, null, 1, 10);

            Assert.Equal(1, result.TotalCount);
            Assert.Equal("Marketing Lead", result.Data[0].Title);
        }

        [Fact]
        public async Task GetJobsAsync_FilterBySalaryMin_ExcludesLowerPaidJobs()
        {
            using var context = CreateContext();
            var service = new JobService(context, CreateMapper());

            var result = await service.GetJobsAsync(null, null, null, 1500m, null, 1, 10);

            Assert.Equal(1, result.TotalCount);
            Assert.Equal("Backend Dev", result.Data[0].Title);
        }

        [Fact]
        public async Task CreateJobAsync_NoEmployerProfile_ReturnsEmployerNotFound()
        {
            using var context = CreateContext();
            var service = new JobService(context, CreateMapper());
            var dto = new CreateJobDto { Title = "New Job", Location = "Baku", Description = "Desc", JobType = "FullTime", Category = "Technology" };

            var result = await service.CreateJobAsync(999, dto);

            Assert.Equal(JobOperationResult.EmployerNotFound, result);
        }

        [Fact]
        public async Task CreateJobAsync_ValidEmployer_CreatesJob()
        {
            using var context = CreateContext();
            var service = new JobService(context, CreateMapper());
            var dto = new CreateJobDto { Title = "New Job", Location = "Baku", Description = "Desc", JobType = "FullTime", Category = "Technology" };

            var result = await service.CreateJobAsync(1, dto);

            Assert.Equal(JobOperationResult.Success, result);
            Assert.Equal(4, await context.JobPosts.CountAsync());
        }

        [Fact]
        public async Task DeleteJobAsync_NotOwner_ReturnsJobNotFound()
        {
            using var context = CreateContext();
            context.Users.Add(new User { Id = 2, Username = "other", Email = "other@test.com", PasswordHash = "x", Role = "Employer" });
            context.EmployerProfiles.Add(new EmployerProfile { Id = 2, UserId = 2, CompanyName = "Other Co" });
            context.SaveChanges();
            var service = new JobService(context, CreateMapper());

            var result = await service.DeleteJobAsync(1, 2);

            Assert.Equal(JobOperationResult.JobNotFound, result);
        }
    }
}