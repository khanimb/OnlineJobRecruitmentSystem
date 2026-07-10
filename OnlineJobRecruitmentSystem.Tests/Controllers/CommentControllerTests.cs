using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineJobRecruitmentSystem.API.Controllers;
using OnlineJobRecruitmentSystem.Application.DTOs.CommentDtos;
using OnlineJobRecruitmentSystem.Application.Validations.CommentDtoValidation;
using OnlineJobRecruitmentSystem.Domain.Entities;
using OnlineJobRecruitmentSystem.Infrastructure.Data;
using System.Security.Claims;

namespace OnlineJobRecruitmentSystem.Tests.Controllers
{
    public class CommentControllerTests
    {
        private static AppDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var context = new AppDbContext(options);

            context.JobPosts.Add(new JobPost
            {
                Id = 1,
                EmployerProfileId = 1,
                Title = "Backend Developer",
                Description = "desc",
                Requirements = "reqs",
                Location = "Baku",
                JobType = "FullTime",
                Category = "IT",
                SalaryMin = 1000,
                SalaryMax = 2000,
                Deadline = DateTime.UtcNow.AddMonths(1)
            });

            context.Users.Add(new User { Id = 1, Username = "jobseeker1", Email = "js1@test.com", PasswordHash = "x", Role = "JobSeeker" });

            context.SaveChanges();
            return context;
        }

        private static CommentController CreateController(AppDbContext context, int userId)
        {
            var controller = new CommentController(
                context,
                new CreateCommentDtoValidation(),
                new UpdateCommentDtoValidation());

            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
            var identity = new ClaimsIdentity(claims, "TestAuth");

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
            };

            return controller;
        }

        [Fact]
        public async Task CreateComment_ExistingJobPost_ReturnsOk()
        {
            using var context = CreateContext();
            var controller = CreateController(context, userId: 1);
            var dto = new CreateCommentDto { JobPostId = 1, Text = "Is this remote?" };

            var result = await controller.CreateComment(dto);

            Assert.IsType<OkObjectResult>(result);
            Assert.Equal(1, context.Comments.Count());
        }

        [Fact]
        public async Task CreateComment_NonExistentJobPost_ReturnsNotFound()
        {
            using var context = CreateContext();
            var controller = CreateController(context, userId: 1);
            var dto = new CreateCommentDto { JobPostId = 999, Text = "Hello" };

            var result = await controller.CreateComment(dto);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task DeleteComment_OwnComment_DeletesSuccessfully()
        {
            using var context = CreateContext();
            context.Comments.Add(new Comment { Id = 1, JobPostId = 1, UserId = 1, Text = "test" });
            context.SaveChanges();
            var controller = CreateController(context, userId: 1);

            var result = await controller.DeleteComment(1);

            Assert.IsType<OkObjectResult>(result);
            Assert.Empty(context.Comments);
        }

        [Fact]
        public async Task DeleteComment_NotOwnComment_ReturnsNotFound()
        {
            using var context = CreateContext();
            context.Comments.Add(new Comment { Id = 1, JobPostId = 1, UserId = 1, Text = "test" });
            context.SaveChanges();
            var controller = CreateController(context, userId: 2);

            var result = await controller.DeleteComment(1);

            Assert.IsType<NotFoundObjectResult>(result);
            Assert.Single(context.Comments);
        }
    }
}