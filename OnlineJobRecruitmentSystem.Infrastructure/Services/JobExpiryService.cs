using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OnlineJobRecruitmentSystem.Application.Interfaces;
using OnlineJobRecruitmentSystem.Infrastructure.Data;

namespace OnlineJobRecruitmentSystem.Infrastructure.Services
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
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                var expiredJobs = await context.JobPosts
                    .Include(j => j.EmployerProfile)
                        .ThenInclude(e => e!.User)
                    .Where(j => j.IsActive && j.Deadline < DateTime.UtcNow)
                    .ToListAsync(stoppingToken);

                foreach (var job in expiredJobs)
                {
                    job.IsActive = false;

                    await emailService.SendEmailAsync(
                        job.EmployerProfile!.User!.Email,
                        "Job Posting Expired",
                        $"Your job posting '{job.Title}' has expired and has been deactivated."
                    );
                }

                if (expiredJobs.Any())
                    await context.SaveChangesAsync(stoppingToken);

                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
    }
}
