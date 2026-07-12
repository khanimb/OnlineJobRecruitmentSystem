using Microsoft.EntityFrameworkCore;
using OnlineJobRecruitmentSystem.Application.DTOs.AccountDtos;
using OnlineJobRecruitmentSystem.Application.Interfaces;
using OnlineJobRecruitmentSystem.Infrastructure.Data;
using QuestPDF.Fluent;
using QuestPDF.Helpers;

namespace OnlineJobRecruitmentSystem.Infrastructure.Services
{
    public class AccountService : IAccountService
    {
        private readonly AppDbContext _context;

        public AccountService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ProfileReturnDto?> GetProfileAsync(int userId)
        {
            var user = await _context.Users
                .Include(u => u.EmployerProfile)
                .Include(u => u.JobSeekerProfile)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return null;

            return new ProfileReturnDto
            {
                UserId = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                CompanyName = user.EmployerProfile?.CompanyName,
                Description = user.EmployerProfile?.Description,
                Website = user.EmployerProfile?.Website,
                LogoUrl = user.EmployerProfile?.LogoUrl,
                FullName = user.JobSeekerProfile?.FullName,
                Phone = user.JobSeekerProfile?.Phone,
                Skills = user.JobSeekerProfile?.Skills,
                WorkExperience = user.JobSeekerProfile?.WorkExperience,
                CvUrl = user.JobSeekerProfile?.CvUrl
            };
        }

        public async Task<CvPdfResult?> GenerateCvPdfAsync(int userId)
        {
            var profile = await _context.JobSeekerProfiles
                .Include(j => j.User)
                .FirstOrDefaultAsync(j => j.UserId == userId);

            if (profile == null) return null;

            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

            var pdf = QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.DefaultTextStyle(x => x.FontSize(12));

                    page.Content().Column(col =>
                    {
                        col.Item().Text(profile.FullName)
                            .FontSize(24).Bold();

                        col.Item().Text(profile.User!.Email)
                            .FontSize(12).FontColor("#666666");

                        col.Item().Text(profile.Phone)
                            .FontSize(12).FontColor("#666666");

                        col.Item().PaddingTop(20).Text("Skills").FontSize(16).Bold();
                        col.Item().Text(profile.Skills);

                        col.Item().PaddingTop(20).Text("Work Experience").FontSize(16).Bold();
                        col.Item().Text(profile.WorkExperience);
                    });
                });
            }).GeneratePdf();

            return new CvPdfResult { PdfBytes = pdf, FileName = $"{profile.FullName}_CV.pdf" };
        }
    }
}