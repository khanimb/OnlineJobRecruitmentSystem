using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineJobRecruitmentSystem.Application.DTOs.AccountDtos;
using OnlineJobRecruitmentSystem.Common;
using OnlineJobRecruitmentSystem.Infrastructure.Data;
using QuestPDF.Fluent;
using QuestPDF.Helpers;

namespace OnlineJobRecruitmentSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AccountController(AppDbContext context, IWebHostEnvironment env) : BaseApiController
    {
        

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = CurrentUserId;

            var user = await context.Users
                .Include(u => u.EmployerProfile)
                .Include(u => u.JobSeekerProfile)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return NotFound(ResponseModel<string>.Fail("User not found."));

            var dto = new ProfileReturnDto
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

            return Ok(ResponseModel<ProfileReturnDto>.Ok(dto));
        }

        [HttpGet("cv/download")]
        [Authorize(Roles = OnlineJobRecruitmentSystem.Domain.Common.Roles.JobSeeker)]
        public async Task<IActionResult> DownloadCvAsPdf()
        {
            var userId = CurrentUserId;

            var profile = await context.JobSeekerProfiles
                .Include(j => j.User)
                .FirstOrDefaultAsync(j => j.UserId == userId);

            if (profile == null)
                return NotFound(ResponseModel<string>.Fail("Job seeker profile not found."));

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

            return File(pdf, "application/pdf", $"{profile.FullName}_CV.pdf");
        }
    }
}