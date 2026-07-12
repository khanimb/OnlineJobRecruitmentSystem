using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineJobRecruitmentSystem.Application.DTOs.CvAnalysisDtos;
using OnlineJobRecruitmentSystem.Application.Interfaces;
using OnlineJobRecruitmentSystem.Common;
using OnlineJobRecruitmentSystem.Domain.Entities;
using OnlineJobRecruitmentSystem.Infrastructure.Data;
using OnlineJobRecruitmentSystem.Infrastructure.Extensions;
using System.Text;
using System.Text.Json;

namespace OnlineJobRecruitmentSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = OnlineJobRecruitmentSystem.Domain.Common.Roles.JobSeeker)]
    public class CvAnalysisController(
        AppDbContext context,
        IGeminiService geminiService,
        FileManager fileManager) : BaseApiController
    {
        

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        [HttpPost("analyze")]
        public async Task<IActionResult> AnalyzeCv(AnalyzeCvDto dto)
        {
            var userId = CurrentUserId;
            var profile = await context.JobSeekerProfiles.FirstOrDefaultAsync(j => j.UserId == userId);
            if (profile == null)
                return NotFound(ResponseModel<string>.Fail("Job seeker profile not found."));

            if (string.IsNullOrEmpty(profile.CvUrl))
                return BadRequest(ResponseModel<string>.Fail("Please upload a CV first."));

            string cvText;
            try
            {
                var physicalPath = fileManager.GetPhysicalPath(profile.CvUrl);
                cvText = CvTextExtractor.ExtractText(physicalPath);
            }
            catch (NotSupportedException ex)
            {
                return BadRequest(ResponseModel<string>.Fail(ex.Message));
            }

            JobPost? job = null;
            if (dto.JobPostId.HasValue)
            {
                job = await context.JobPosts.FirstOrDefaultAsync(j => j.Id == dto.JobPostId.Value);
                if (job == null)
                    return NotFound(ResponseModel<string>.Fail("Job post not found."));
            }

            var prompt = BuildAnalysisPrompt(cvText, job);
            var jsonResponse = await geminiService.GenerateContentAsync(prompt);

            var result = JsonSerializer.Deserialize<CvAnalysisResultDto>(jsonResponse, JsonOptions);

            context.CvAnalysisHistories.Add(new CvAnalysisHistory
            {
                JobSeekerProfileId = profile.Id,
                JobPostId = dto.JobPostId,
                OverallScore = result!.OverallScore,
                MatchScore = result.MatchScore,
                ResultJson = jsonResponse
            });
            await context.SaveChangesAsync();

            return Ok(ResponseModel<CvAnalysisResultDto>.Ok(result));
        }

        [HttpGet("recommended-jobs")]
        public async Task<IActionResult> GetRecommendedJobs()
        {
            var userId = CurrentUserId;
            var profile = await context.JobSeekerProfiles.FirstOrDefaultAsync(j => j.UserId == userId);
            if (profile == null)
                return NotFound(ResponseModel<string>.Fail("Job seeker profile not found."));

            if (string.IsNullOrEmpty(profile.CvUrl))
                return BadRequest(ResponseModel<string>.Fail("Please upload a CV first."));

            var physicalPath = fileManager.GetPhysicalPath(profile.CvUrl);
            var cvText = CvTextExtractor.ExtractText(physicalPath);

            var activeJobs = await context.JobPosts
                .Include(j => j.EmployerProfile)
                .Where(j => j.IsActive)
                .OrderByDescending(j => j.CreatedAt)
                .Take(20)
                .ToListAsync();

            if (activeJobs.Count == 0)
                return Ok(ResponseModel<List<JobRecommendationDto>>.Ok(new List<JobRecommendationDto>()));

            var prompt = BuildRecommendationPrompt(cvText, activeJobs);
            var jsonResponse = await geminiService.GenerateContentAsync(prompt);

            var parsed = JsonSerializer.Deserialize<GeminiRecommendationResponseDto>(jsonResponse, JsonOptions);

            var result = parsed!.Recommendations
                .Select(r =>
                {
                    var job = activeJobs.FirstOrDefault(j => j.Id == r.JobPostId);
                    return job == null ? null : new JobRecommendationDto
                    {
                        JobPostId = job.Id,
                        JobTitle = job.Title,
                        CompanyName = job.EmployerProfile!.CompanyName,
                        MatchScore = r.MatchScore,
                        Reason = r.Reason
                    };
                })
                .Where(r => r != null)
                .OrderByDescending(r => r!.MatchScore)
                .ToList();

            return Ok(ResponseModel<List<JobRecommendationDto>>.Ok(result!));
        }

        private static string BuildAnalysisPrompt(string cvText, JobPost? job)
        {
            var sb = new StringBuilder();
            sb.AppendLine("You are a professional CV reviewer for a job recruitment platform.");
            sb.AppendLine("Analyze the following CV and respond ONLY with valid JSON, no markdown, matching exactly this schema:");
            sb.AppendLine(job == null
                ? "{ \"overallScore\": <int 0-100>, \"strengths\": [string], \"weaknesses\": [string], \"suggestions\": [string] }"
                : "{ \"overallScore\": <int 0-100>, \"strengths\": [string], \"weaknesses\": [string], \"suggestions\": [string], \"matchScore\": <int 0-100>, \"missingSkills\": [string] }");
            sb.AppendLine();
            sb.AppendLine("CV content:");
            sb.AppendLine("\"\"\"");
            sb.AppendLine(cvText);
            sb.AppendLine("\"\"\"");

            if (job != null)
            {
                sb.AppendLine();
                sb.AppendLine("Also match this CV against the following job posting, and include matchScore and missingSkills:");
                sb.AppendLine($"Title: {job.Title}");
                sb.AppendLine($"Description: {job.Description}");
                sb.AppendLine($"Requirements: {job.Requirements}");
            }

            return sb.ToString();
        }

        private static string BuildRecommendationPrompt(string cvText, List<JobPost> jobs)
        {
            var sb = new StringBuilder();
            sb.AppendLine("You are a job-matching assistant for a recruitment platform.");
            sb.AppendLine("Given the candidate's CV and the list of available job postings below, recommend up to 5 best-matching jobs.");
            sb.AppendLine("Respond ONLY with valid JSON, no markdown, matching exactly this schema:");
            sb.AppendLine("{ \"recommendations\": [ { \"jobPostId\": <int>, \"matchScore\": <int 0-100>, \"reason\": <string> } ] }");
            sb.AppendLine();
            sb.AppendLine("Candidate CV:");
            sb.AppendLine("\"\"\"");
            sb.AppendLine(cvText);
            sb.AppendLine("\"\"\"");
            sb.AppendLine();
            sb.AppendLine("Available jobs:");
            foreach (var job in jobs)
                sb.AppendLine($"[Id: {job.Id}] Title: {job.Title} | Location: {job.Location} | Requirements: {job.Requirements}");

            return sb.ToString();
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetHistory()
        {
            var userId = CurrentUserId;
            var profile = await context.JobSeekerProfiles.FirstOrDefaultAsync(j => j.UserId == userId);
            if (profile == null)
                return NotFound(ResponseModel<string>.Fail("Job seeker profile not found."));

            var history = await context.CvAnalysisHistories
                .Include(h => h.JobPost)
                .Where(h => h.JobSeekerProfileId == profile.Id)
                .OrderByDescending(h => h.CreatedAt)
                .Select(h => new ReturnCvAnalysisHistoryDto
                {
                    Id = h.Id,
                    JobPostId = h.JobPostId,
                    JobTitle = h.JobPost == null ? null : h.JobPost.Title,
                    OverallScore = h.OverallScore,
                    MatchScore = h.MatchScore,
                    CreatedAt = h.CreatedAt
                })
                .ToListAsync();

            return Ok(ResponseModel<List<ReturnCvAnalysisHistoryDto>>.Ok(history));
        }

        [HttpGet("history/{id}")]
        public async Task<IActionResult> GetHistoryDetail(int id)
        {
            var userId = CurrentUserId;
            var profile = await context.JobSeekerProfiles.FirstOrDefaultAsync(j => j.UserId == userId);
            if (profile == null)
                return NotFound(ResponseModel<string>.Fail("Job seeker profile not found."));

            var history = await context.CvAnalysisHistories
                .FirstOrDefaultAsync(h => h.Id == id && h.JobSeekerProfileId == profile.Id);

            if (history == null)
                return NotFound(ResponseModel<string>.Fail("History not found."));

            var result = JsonSerializer.Deserialize<CvAnalysisResultDto>(history.ResultJson, JsonOptions);
            if (result == null)
                return NotFound(ResponseModel<string>.Fail("Analysis result could not be read."));

            return Ok(ResponseModel<CvAnalysisResultDto>.Ok(result));
        }

        [HttpDelete("history/{id}")]
        public async Task<IActionResult> DeleteHistory(int id)
        {
            var userId = CurrentUserId;
            var profile = await context.JobSeekerProfiles.FirstOrDefaultAsync(j => j.UserId == userId);
            if (profile == null)
                return NotFound(ResponseModel<string>.Fail("Job seeker profile not found."));

            var history = await context.CvAnalysisHistories
                .FirstOrDefaultAsync(h => h.Id == id && h.JobSeekerProfileId == profile.Id);

            if (history == null)
                return NotFound(ResponseModel<string>.Fail("History not found."));

            context.CvAnalysisHistories.Remove(history);
            await context.SaveChangesAsync();

            return Ok(ResponseModel<string>.Ok(null!, "History deleted."));
        }
    }
}