using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using OnlineJobRecruitmentSystem.API.Hubs;
using OnlineJobRecruitmentSystem.Application.DTOs.MessageDtos;
using OnlineJobRecruitmentSystem.Application.Interfaces;
using OnlineJobRecruitmentSystem.Common;

namespace OnlineJobRecruitmentSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MessageController(
        IMessageService messageService,
        IHubContext<ChatHub> chatHub,
        IValidator<SendMessageDto> validator
        ) : BaseApiController
    {


        [HttpPost]
        public async Task<IActionResult> SendMessage(SendMessageDto dto)
        {
            var result = await validator.ValidateAsync(dto);
            if (!result.IsValid)
                return BadRequest(ResponseModel<string>.Fail(result.Errors[0].ErrorMessage));

            var senderId = CurrentUserId;
            var message = await messageService.SaveMessageAsync(senderId, dto);

            await chatHub.Clients.User(dto.ReceiverId.ToString())
                .SendAsync("ReceiveMessage", new
                {
                    message.Id,
                    message.SenderId,
                    message.Content,
                    message.CreatedAt
                });

            return Ok(ResponseModel<string>.Ok(null!, "Message sent."));
        }

        [HttpGet("conversation/{otherUserId}")]
        public async Task<IActionResult> GetConversation(int otherUserId)
        {
            var messages = await messageService.GetConversationAsync(CurrentUserId, otherUserId);
            return Ok(ResponseModel<List<ReturnMessageDto>>.Ok(messages));
        }

        [HttpGet("inbox")]
        public async Task<IActionResult> GetInbox()
        {
            var messages = await messageService.GetInboxAsync(CurrentUserId);
            return Ok(ResponseModel<List<ReturnMessageDto>>.Ok(messages));
        }

        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var count = await messageService.GetUnreadCountAsync(CurrentUserId);
            return Ok(ResponseModel<int>.Ok(count));
        }
    }
}