using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineJobRecruitmentSystem.Data;
using OnlineJobRecruitmentSystem.DTOs.AccountDtos;
using OnlineJobRecruitmentSystem.Models;
using QuestPDF.Fluent;
using System.Security.Claims;
using QuestPDF.Helpers;

namespace OnlineJobRecruitmentSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AccountController(AppDbContext context, IWebHostEnvironment env) : ControllerBase
    {
        private int GetUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = GetUserId();

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

        [HttpPost("cv")]
        [Authorize(Roles = "JobSeeker")]
        public async Task<IActionResult> UploadCv(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(ResponseModel<string>.Fail("Please select a file."));

            var allowed = new[] { ".pdf", ".doc", ".docx" };
            var ext = Path.GetExtension(file.FileName).ToLower();

            if (!allowed.Contains(ext))
                return BadRequest(ResponseModel<string>.Fail("Only PDF, DOC, DOCX files are allowed."));

            var userId = GetUserId();

            var profile = await context.JobSeekerProfiles.FirstOrDefaultAsync(j => j.UserId == userId);
            if (profile == null)
                return NotFound(ResponseModel<string>.Fail("Job seeker profile not found."));

            var uploadPath = Path.Combine(env.WebRootPath ?? "wwwroot", "cvs");
            Directory.CreateDirectory(uploadPath);

            var fileName = $"cv_{userId}_{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(uploadPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
                await file.CopyToAsync(stream);

            profile.CvUrl = $"/cvs/{fileName}";
            await context.SaveChangesAsync();

            return Ok(ResponseModel<string>.Ok(profile.CvUrl, "CV uploaded successfully."));
        }

        [HttpGet("cv/download")]
        [Authorize(Roles = "JobSeeker")]
        public async Task<IActionResult> DownloadCvAsPdf()
        {
            var userId = GetUserId();

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