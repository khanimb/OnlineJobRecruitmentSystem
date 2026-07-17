using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineJobRecruitmentSystem.Application.DTOs.ContactDtos;
using OnlineJobRecruitmentSystem.Application.Interfaces;
using OnlineJobRecruitmentSystem.Common;

namespace OnlineJobRecruitmentSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class ContactController(IContactService contactService) : BaseApiController
    {
        [HttpPost]
        public async Task<IActionResult> Send(ContactMessageDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Message))
                return BadRequest(ResponseModel<string>.Fail("Please fill in all fields."));

            await contactService.SendContactMessageAsync(dto);
            return Ok(ResponseModel<string>.Ok(null!, "Message sent! We will get back to you soon."));
        }
    }
}