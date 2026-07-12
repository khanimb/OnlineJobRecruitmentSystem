using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OnlineJobRecruitmentSystem.API.Controllers;
using OnlineJobRecruitmentSystem.Application.DTOs.JobDtos;
using OnlineJobRecruitmentSystem.Application.Profiles;
using OnlineJobRecruitmentSystem.Application.Validations.JobDtoValidation;
using OnlineJobRecruitmentSystem.Domain.Entities;
using OnlineJobRecruitmentSystem.Infrastructure.Data;
using OnlineJobRecruitmentSystem.Infrastructure.Services;
using System.Security.Claims;

namespace OnlineJobRecruitmentSystem.Tests.Controllers
{
    public class JobControllerTests
    {
        private static AppDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var context = new AppDbContext(options);

            context.Users.Add(new User { Id = 1, Username = "employer1", Email = "e1@test.com", PasswordHash = "x", Role = "Employer" });
            context.EmployerProfiles.Add(new EmployerProfile { Id = 1, UserId = 1, CompanyName = "TestCo" });
            context.SaveChanges();

            return context;
        }

        private static JobController CreateController(AppDbContext context, int userId)
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddAutoMapper(cfg => cfg.AddProfile<MapperProfile>());
            var mapper = services.BuildServiceProvider().GetRequiredService<IMapper>();

            var jobService = new JobService(context, mapper);
            var controller = new JobController(jobService, new CreateJobDtoValidation(), new UpdateJobDtoValidation());

            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
            var identity = new ClaimsIdentity(claims, "TestAuth");

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
            };

            return controller;
        }

        private static CreateJobDto ValidJobDto() => new()
        {
            Title = "Backend Developer",
            Description = "desc",
            Location = "Baku",
            JobType = "FullTime",
            Category = "IT",
            SalaryMin = 1000,
            SalaryMax = 2000,
            Deadline = DateTime.UtcNow.AddMonths(1)
        };

        [Fact]
        public async Task CreateJob_ValidDto_CreatesJobSuccessfully()
        {
            using var context = CreateContext();
            var controller = CreateController(context, userId: 1);

            var result = await controller.CreateJob(ValidJobDto());

            Assert.IsType<OkObjectResult>(result);
            Assert.Equal(1, context.JobPosts.Count());
        }

        [Fact]
        public async Task CreateJob_NoEmployerProfile_ReturnsBadRequest()
        {
            using var context = CreateContext();
            var controller = CreateController(context, userId: 999);

            var result = await controller.CreateJob(ValidJobDto());

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task GetJobs_OnlyReturnsActiveJobs()
        {
            using var context = CreateContext();
            context.JobPosts.AddRange(
                new JobPost { EmployerProfileId = 1, Title = "Active Job", Description = "d", Location = "Baku", JobType = "FullTime", IsActive = true },
                new JobPost { EmployerProfileId = 1, Title = "Inactive Job", Description = "d", Location = "Baku", JobType = "FullTime", IsActive = false }
            );
            context.SaveChanges();
            var controller = CreateController(context, userId: 1);

            var result = await controller.GetJobs(null, null, null, null, null, 1, 10);
            var ok = Assert.IsType<OkObjectResult>(result);
            dynamic response = ok.Value!;

            Assert.Equal(1, (int)response.Data.TotalCount);
        }

        [Fact]
        public async Task GetJobs_FilterByCategory_ReturnsOnlyMatching()
        {
            using var context = CreateContext();
            context.JobPosts.AddRange(
                new JobPost { EmployerProfileId = 1, Title = "IT Job", Description = "d", Location = "Baku", JobType = "FullTime", Category = "IT", IsActive = true },
                new JobPost { EmployerProfileId = 1, Title = "Sales Job", Description = "d", Location = "Baku", JobType = "FullTime", Category = "Sales", IsActive = true }
            );
            context.SaveChanges();
            var controller = CreateController(context, userId: 1);

            var result = await controller.GetJobs("IT", null, null, null, null, 1, 10);
            var ok = Assert.IsType<OkObjectResult>(result);
            dynamic response = ok.Value!;

            Assert.Equal(1, (int)response.Data.TotalCount);
        }

        [Fact]
        public async Task DeleteJob_Owner_DeletesSuccessfully()
        {
            using var context = CreateContext();
            context.JobPosts.Add(new JobPost { Id = 1, EmployerProfileId = 1, Title = "Job", Description = "d", Location = "Baku", JobType = "FullTime" });
            context.SaveChanges();
            var controller = CreateController(context, userId: 1);

            var result = await controller.DeleteJob(1);

            Assert.IsType<OkObjectResult>(result);
            Assert.Empty(context.JobPosts);
        }

        [Fact]
        public async Task DeleteJob_NotOwner_ReturnsNotFound()
        {
            using var context = CreateContext();
            context.Users.Add(new User { Id = 2, Username = "employer2", Email = "e2@test.com", PasswordHash = "x", Role = "Employer" });
            context.EmployerProfiles.Add(new EmployerProfile { Id = 2, UserId = 2, CompanyName = "OtherCo" });
            context.JobPosts.Add(new JobPost { Id = 1, EmployerProfileId = 1, Title = "Job", Description = "d", Location = "Baku", JobType = "FullTime" });
            context.SaveChanges();
            var controller = CreateController(context, userId: 2);

            var result = await controller.DeleteJob(1);

            Assert.IsType<NotFoundObjectResult>(result);
            Assert.Single(context.JobPosts);
        }

        [Fact]
        public async Task GetSimilarJobs_ReturnsSameCategoryExcludingSelf()
        {
            using var context = CreateContext();
            context.JobPosts.AddRange(
                new JobPost { Id = 1, EmployerProfileId = 1, Title = "Job A", Description = "d", Location = "Baku", JobType = "FullTime", Category = "IT", IsActive = true },
                new JobPost { Id = 2, EmployerProfileId = 1, Title = "Job B", Description = "d", Location = "Baku", JobType = "FullTime", Category = "IT", IsActive = true },
                new JobPost { Id = 3, EmployerProfileId = 1, Title = "Job C", Description = "d", Location = "Baku", JobType = "PartTime", Category = "Sales", IsActive = true }
            );
            context.SaveChanges();
            var controller = CreateController(context, userId: 1);

            var result = await controller.GetSimilarJobs(1);
            var ok = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<OnlineJobRecruitmentSystem.Common.ResponseModel<List<ReturnJobDto>>>(ok.Value);

            Assert.Single(response.Data!);
            Assert.Equal("Job B", response.Data![0].Title);
        }
    }
}