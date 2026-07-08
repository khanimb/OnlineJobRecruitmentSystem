using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OnlineJobRecruitmentSystem.API.Hubs;
using OnlineJobRecruitmentSystem.Application.DTOs.ContractPaymentDtos;
using OnlineJobRecruitmentSystem.Application.DTOs.NotificationDtos;
using OnlineJobRecruitmentSystem.Common;
using OnlineJobRecruitmentSystem.Domain.Entities;
using OnlineJobRecruitmentSystem.Domain.Enums;
using OnlineJobRecruitmentSystem.Infrastructure.Data;
using Stripe.Checkout;
using System.Security.Claims;

namespace OnlineJobRecruitmentSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ContractPaymentController(
        AppDbContext context,
        IHubContext<NotificationHub> notificationHub,
        IConfiguration configuration,
        ILogger<ContractPaymentController> logger) : BaseApiController
    {
        

        [HttpPost("{contractId}")]
        [Authorize(Roles = OnlineJobRecruitmentSystem.Domain.Common.Roles.Employer)]
        public async Task<IActionResult> CreatePayment(int contractId)
        {
            var userId = CurrentUserId;
            var employer = await context.EmployerProfiles
                .FirstOrDefaultAsync(e => e.UserId == userId);

            if (employer == null)
                return NotFound(ResponseModel<string>.Fail("Employer profile not found."));

            var contract = await context.Contracts
                .Include(c => c.JobPost)
                .FirstOrDefaultAsync(c => c.Id == contractId && c.EmployerProfileId == employer.Id);

            if (contract == null)
                return NotFound(ResponseModel<string>.Fail("Contract not found."));

            if (contract.Status != ContractStatus.Completed)
                return BadRequest(ResponseModel<string>.Fail("Contract must be completed before payment."));

            var alreadyPaid = await context.ContractPayments
                .AnyAsync(cp => cp.ContractId == contractId && cp.Status == ContractPaymentStatus.Completed);

            if (alreadyPaid)
                return BadRequest(ResponseModel<string>.Fail("This contract has already been paid."));

            var platformFee = contract.Amount * 0.10m;
            var jobSeekerAmount = contract.Amount - platformFee;

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = "usd",
                            UnitAmount = (long)(contract.Amount * 100),
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = $"Payment for: {contract.JobPost.Title}"
                            }
                        },
                        Quantity = 1
                    }
                },
                Mode = "payment",
                SuccessUrl = $"{configuration["App:BaseUrl"]}/assets/pages/paymentsuccess.html?session_id={{CHECKOUT_SESSION_ID}}",
                CancelUrl = $"{configuration["App:BaseUrl"]}/assets/pages/paymentcancel.html",
                Metadata = new Dictionary<string, string>
                {
                    { "contractId", contract.Id.ToString() },
                    { "platformFee", platformFee.ToString() },
                    { "jobSeekerAmount", jobSeekerAmount.ToString() }
                }
            };

            var service = new SessionService();
            var session = await service.CreateAsync(options);

            var contractPayment = new ContractPayment
            {
                ContractId = contract.Id,
                StripePaymentId = session.Id,
                TotalAmount = contract.Amount,
                PlatformFee = platformFee,
                JobSeekerAmount = jobSeekerAmount,
                Status = ContractPaymentStatus.Pending
            };

            context.ContractPayments.Add(contractPayment);
            await context.SaveChangesAsync();

            return Ok(ResponseModel<object>.Ok(new
            {
                sessionId = session.Id,
                url = session.Url
            }));
        }

        [HttpGet("{contractId}")]
        public async Task<IActionResult> GetPayments(int contractId)
        {
            var userId = CurrentUserId;

            var contract = await context.Contracts
                .Include(c => c.EmployerProfile)
                .Include(c => c.JobSeekerProfile)
                .FirstOrDefaultAsync(c => c.Id == contractId);

            if (contract == null)
                return NotFound(ResponseModel<string>.Fail("Contract not found."));

            if (contract.EmployerProfile.UserId != userId && contract.JobSeekerProfile.UserId != userId)
                return Forbid();

            var payments = await context.ContractPayments
                .Where(cp => cp.ContractId == contractId)
                .Select(cp => new ReturnContractPaymentDto
                {
                    Id = cp.Id,
                    ContractId = cp.ContractId,
                    StripePaymentId = cp.StripePaymentId,
                    TotalAmount = cp.TotalAmount,
                    PlatformFee = cp.PlatformFee,
                    JobSeekerAmount = cp.JobSeekerAmount,
                    Status = cp.Status,
                    CreatedAt = cp.CreatedAt
                })
                .ToListAsync();

            return Ok(ResponseModel<List<ReturnContractPaymentDto>>.Ok(payments));
        }

        [HttpPost("webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> Webhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var webhookSecret = configuration["Stripe:WebhookSecret"];

            try
            {
                var stripeEvent = Stripe.EventUtility.ConstructEvent(
                    json,
                    Request.Headers["Stripe-Signature"],
                    webhookSecret
                );

                if (stripeEvent.Type == "checkout.session.completed")
                {
                    var session = stripeEvent.Data.Object as Session;
                    if (session == null) return Ok();

                    var contractPayment = await context.ContractPayments
                        .Include(cp => cp.Contract)
                        .FirstOrDefaultAsync(cp => cp.StripePaymentId == session.Id);

                    if (contractPayment != null)
                    {
                        contractPayment.Status = ContractPaymentStatus.Completed;
                        await context.SaveChangesAsync();

                        var jobSeeker = await context.JobSeekerProfiles
                            .Include(j => j.User)
                            .FirstOrDefaultAsync(j => j.Id == contractPayment.Contract.JobSeekerProfileId);

                        if (jobSeeker != null)
                        {
                            var notification = new Notification
                            {
                                UserId = jobSeeker.UserId,
                                Title = "Payment Received",
                                Message = $"You have received ${contractPayment.JobSeekerAmount} for your work.",
                                Type = "payment"
                            };

                            context.Notifications.Add(notification);
                            await context.SaveChangesAsync();

                            await notificationHub.Clients
                                .Group($"user_{jobSeeker.UserId}")
                                .SendAsync("ReceiveNotification", new ReturnNotificationDto
                                {
                                    Id = notification.Id,
                                    Title = notification.Title,
                                    Message = notification.Message,
                                    IsRead = false,
                                    Type = notification.Type,
                                    CreatedAt = notification.CreatedAt
                                });
                        }
                    }
                }

                return Ok();
            }
            catch (Stripe.StripeException ex)
            {
                logger.LogError(ex, "Stripe payment failed");
                return BadRequest(ResponseModel<string>.Fail("Payment processing failed."));
            }
        }
    }
}