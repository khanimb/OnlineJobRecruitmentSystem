using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineJobRecruitmentSystem.Application.DTOs.PaymentDtos;
using OnlineJobRecruitmentSystem.Common;
using OnlineJobRecruitmentSystem.Domain.Entities;
using OnlineJobRecruitmentSystem.Infrastructure.Data;
using Stripe.Checkout;
using System.Security.Claims;

namespace OnlineJobRecruitmentSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PaymentController(AppDbContext context, IConfiguration configuration) : ControllerBase
    {
        private int GetUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        [HttpPost("create-checkout-session")]
        public async Task<IActionResult> CreateCheckoutSession(CreatePaymentDto dto)
        {
            var plans = new Dictionary<string, (decimal amount, string name, int months)>
            {
                { "basic",   (9.99m,  "Basic Plan - 1 Month Premium",  1) },
                { "standard",(29.99m, "Standard Plan - 3 Month Premium", 3) },
                { "premium", (49.99m, "Premium Plan - 6 Month Premium", 6) }
            };

            if (!plans.TryGetValue(dto.Plan ?? "", out var plan))
                return BadRequest(ResponseModel<string>.Fail("Invalid plan selected."));

            var userId = GetUserId();

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
                            UnitAmount = (long)(plan.amount * 100),
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = plan.name
                            }
                        },
                        Quantity = 1
                    }
                },
                Mode = "payment",
                SuccessUrl = $"http://localhost:5179/assets/pages/payment-success.html?session_id={{CHECKOUT_SESSION_ID}}",
                CancelUrl = $"http://localhost:5179/assets/pages/payment-cancel.html",
                Metadata = new Dictionary<string, string>
                {
                    { "userId", userId.ToString() },
                    { "plan", dto.Plan ?? "" },
                    { "months", plan.months.ToString() }
                }
            };

            var service = new SessionService();
            var session = await service.CreateAsync(options);

            var payment = new Payment
            {
                EmployerId = userId,
                StripePaymentId = session.Id,
                Amount = plan.amount,
                Status = "pending",
                Plan = dto.Plan ?? string.Empty
            };

            context.Payments.Add(payment);
            await context.SaveChangesAsync();

            return Ok(ResponseModel<object>.Ok(new
            {
                sessionId = session.Id,
                url = session.Url
            }));
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
                    var session = stripeEvent.Data.Object as Stripe.Checkout.Session;
                    if (session == null) return Ok();

                    var payment = await context.Payments
                        .FirstOrDefaultAsync(p => p.StripePaymentId == session.Id);

                    if (payment != null)
                    {
                        payment.Status = "completed";

                        if (session.Metadata.TryGetValue("userId", out var userIdStr) &&
                            session.Metadata.TryGetValue("months", out var monthsStr))
                        {
                            var user = await context.Users.FindAsync(int.Parse(userIdStr));
                            if (user != null)
                            {
                                var months = int.Parse(monthsStr);
                                user.IsPremium = true;
                                user.PremiumExpiryDate = DateTime.UtcNow.AddMonths(months);
                            }
                        }

                        await context.SaveChangesAsync();
                    }
                }

                return Ok();
            }
            catch (Stripe.StripeException)
            {
                return BadRequest();
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetPayments()
        {
            var userId = GetUserId();

            var payments = await context.Payments
                .Where(p => p.EmployerId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new ReturnPaymentDto
                {
                    Id = p.Id,
                    Plan = p.Plan,
                    Amount = p.Amount,
                    Status = p.Status,
                    StripePaymentId = p.StripePaymentId,
                    CreatedAt = p.CreatedAt
                })
                .ToListAsync();

            return Ok(ResponseModel<List<ReturnPaymentDto>>.Ok(payments));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPayment(int id)
        {
            var userId = GetUserId();

            var payment = await context.Payments
                .FirstOrDefaultAsync(p => p.Id == id && p.EmployerId == userId);

            if (payment == null)
                return NotFound(ResponseModel<string>.Fail("Payment not found."));

            return Ok(ResponseModel<ReturnPaymentDto>.Ok(new ReturnPaymentDto
            {
                Id = payment.Id,
                Plan = payment.Plan,
                Amount = payment.Amount,
                Status = payment.Status,
                StripePaymentId = payment.StripePaymentId,
                CreatedAt = payment.CreatedAt
            }));
        }

        [HttpGet("status")]
        public async Task<IActionResult> GetPremiumStatus()
        {
            var userId = GetUserId();
            var user = await context.Users.FindAsync(userId);

            if (user == null)
                return NotFound(ResponseModel<string>.Fail("User not found."));

            return Ok(ResponseModel<object>.Ok(new
            {
                isPremium = user.IsPremium,
                expiryDate = user.PremiumExpiryDate
            }));
        }
    }
}