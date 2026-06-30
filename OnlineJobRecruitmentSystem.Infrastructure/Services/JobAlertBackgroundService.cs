using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OnlineJobRecruitmentSystem.Application.Interfaces;
using OnlineJobRecruitmentSystem.Infrastructure.Data;

namespace OnlineJobRecruitmentSystem.Infrastructure.Services
{
    public class JobAlertBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public JobAlertBackgroundService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.UtcNow;

                if (now.Hour == 8 && now.Minute < 5)
                {
                    await SendAlertsAsync("daily", stoppingToken);

                    if (now.DayOfWeek == DayOfWeek.Monday)
                        await SendAlertsAsync("weekly", stoppingToken);
                }

                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }

        private async Task SendAlertsAsync(string frequency, CancellationToken stoppingToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

            var alerts = await context.JobAlerts
                .Include(a => a.User)
                .Where(a => a.IsActive && a.Frequency == frequency)
                .ToListAsync(stoppingToken);

            foreach (var alert in alerts)
            {
                var jobs = await context.JobPosts
                    .Where(j => j.IsActive &&
                        (string.IsNullOrEmpty(alert.Keyword) || j.Title.Contains(alert.Keyword)) &&
                        (string.IsNullOrEmpty(alert.Location) || j.Location.Contains(alert.Location)))
                    .Take(5)
                    .ToListAsync(stoppingToken);

                if (jobs.Any())
                {
                    var jobList = string.Join("<br/>", jobs.Select(j =>
                        $"<li><b>{j.Title}</b> — {j.Location}</li>"));

                    await emailService.SendEmailAsync(
                        alert.User.Email,
                        $"Your {frequency} job alerts",
                        $"<h3>New jobs matching your alert:</h3><ul>{jobList}</ul>"
                    );
                }
            }
        }
    }
}