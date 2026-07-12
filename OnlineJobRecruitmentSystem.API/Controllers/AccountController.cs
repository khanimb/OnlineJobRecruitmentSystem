using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineJobRecruitmentSystem.Application.DTOs.AccountDtos;
using OnlineJobRecruitmentSystem.Application.Interfaces;
using OnlineJobRecruitmentSystem.Common;

namespace OnlineJobRecruitmentSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AccountController(IAccountService accountService) : BaseApiController
    {
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var profile = await accountService.GetProfileAsync(CurrentUserId);
            if (profile == null)
                return NotFound(ResponseModel<string>.Fail("User not found."));

            return Ok(ResponseModel<ProfileReturnDto>.Ok(profile));
        }

        [HttpGet("cv/download")]
        [Authorize(Roles = OnlineJobRecruitmentSystem.Domain.Common.Roles.JobSeeker)]
        public async Task<IActionResult> DownloadCvAsPdf()
        {
            var result = await accountService.GenerateCvPdfAsync(CurrentUserId);
            if (result == null)
                return NotFound(ResponseModel<string>.Fail("Job seeker profile not found."));

            return File(result.PdfBytes, "application/pdf", result.FileName);
        }
    }
}