using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OnlineJobRecruitmentSystem.Application.Interfaces;

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
            var jobAlertService = scope.ServiceProvider.GetRequiredService<IJobAlertService>();
            await jobAlertService.SendJobAlertsAsync(frequency);
        }
    }
}