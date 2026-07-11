using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OnlineJobRecruitmentSystem.Application.DTOs.ReviewDtos;
using OnlineJobRecruitmentSystem.Application.Profiles;
using OnlineJobRecruitmentSystem.Domain.Entities;
using OnlineJobRecruitmentSystem.Infrastructure.Data;
using OnlineJobRecruitmentSystem.Infrastructure.Services;

namespace OnlineJobRecruitmentSystem.Tests.Services
{
    public class ReviewServiceTests
    {
        private static AppDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var context = new AppDbContext(options);

            context.Users.AddRange(
                new User { Id = 1, Username = "reviewer", Email = "reviewer@test.com", PasswordHash = "x", Role = "JobSeeker" },
                new User { Id = 2, Username = "reviewee", Email = "reviewee@test.com", PasswordHash = "x", Role = "Employer" }
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
        public async Task CreateReviewAsync_SelfReview_ReturnsNull()
        {
            using var context = CreateContext();
            var service = new ReviewService(context, CreateMapper());
            var dto = new CreateReviewDto { RevieweeId = 1, Rating = 5, Comment = "Nice" };

            var result = await service.CreateReviewAsync(1, dto);

            Assert.Null(result);
        }

        [Fact]
        public async Task CreateReviewAsync_NonExistentReviewee_ReturnsNull()
        {
            using var context = CreateContext();
            var service = new ReviewService(context, CreateMapper());
            var dto = new CreateReviewDto { RevieweeId = 999, Rating = 5, Comment = "Nice" };

            var result = await service.CreateReviewAsync(1, dto);

            Assert.Null(result);
        }

        [Fact]
        public async Task CreateReviewAsync_ValidReview_CreatesAndReturnsDto()
        {
            using var context = CreateContext();
            var service = new ReviewService(context, CreateMapper());
            var dto = new CreateReviewDto { RevieweeId = 2, Rating = 4, Comment = "Good work" };

            var result = await service.CreateReviewAsync(1, dto);

            Assert.NotNull(result);
            Assert.Equal(4, result!.Rating);
            Assert.Equal(2, result.RevieweeId);
        }

        [Fact]
        public async Task GetAverageRatingAsync_NoReviews_ReturnsZero()
        {
            using var context = CreateContext();
            var service = new ReviewService(context, CreateMapper());

            var avg = await service.GetAverageRatingAsync(2);

            Assert.Equal(0, avg);
        }

        [Fact]
        public async Task GetAverageRatingAsync_WithReviews_ReturnsCorrectAverage()
        {
            using var context = CreateContext();
            var service = new ReviewService(context, CreateMapper());
            await service.CreateReviewAsync(1, new CreateReviewDto { RevieweeId = 2, Rating = 4, Comment = "A" });

            var avg = await service.GetAverageRatingAsync(2);

            Assert.Equal(4, avg);
        }
    }
}