using Microsoft.EntityFrameworkCore;
using OnlineJobRecruitmentSystem.Data;

namespace OnlineJobRecruitmentSystem.Services
{
    public class JobExpiryService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public JobExpiryService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var expiredJobs = await context.JobPosts
                    .Where(j => j.IsActive && j.Deadline < DateTime.UtcNow)
                    .ToListAsync(stoppingToken);

                foreach (var job in expiredJobs)
                    job.IsActive = false;

                if (expiredJobs.Any())
                    await context.SaveChangesAsync(stoppingToken);

                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
    }
}
