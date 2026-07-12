using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineJobRecruitmentSystem.Application.DTOs.PaymentDtos;
using OnlineJobRecruitmentSystem.Application.Interfaces;
using OnlineJobRecruitmentSystem.Common;
using OnlineJobRecruitmentSystem.Infrastructure.Data;
using Stripe.Checkout;

namespace OnlineJobRecruitmentSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PaymentPremiumController(
    AppDbContext context,
    IPaymentService paymentService,
    IConfiguration configuration,
    ILogger<PaymentPremiumController> logger,
    IValidator<CreatePaymentDto> validator) : BaseApiController 
    {
        [HttpPost("create-checkout-session")]
        public async Task<IActionResult> CreateCheckoutSession(CreatePaymentDto dto)
        {
            var result = await validator.ValidateAsync(dto);
            if (!result.IsValid)
                return BadRequest(ResponseModel<string>.Fail(result.Errors[0].ErrorMessage));

            var plans = new Dictionary<string, (decimal amount, string name, int months)>
        {
            { "basic",   (9.99m,  "Basic Plan - 1 Month Premium",  1) },
            { "standard",(29.99m, "Standard Plan - 3 Month Premium", 3) },
            { "premium", (49.99m, "Premium Plan - 6 Month Premium", 6) }
        };

            if (!plans.TryGetValue(dto.Plan ?? "", out var plan))
                return BadRequest(ResponseModel<string>.Fail("Invalid plan selected."));

            var userId = CurrentUserId;

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
                SuccessUrl = $"{configuration["App:BaseUrl"]}/assets/pages/paymentsuccess.html?session_id={{CHECKOUT_SESSION_ID}}",
                CancelUrl = $"{configuration["App:BaseUrl"]}/assets/pages/paymentcancel.html",
                Metadata = new Dictionary<string, string>
                {
                    { "userId", userId.ToString() },
                    { "plan", dto.Plan ?? "" },
                    { "months", plan.months.ToString() }
                }
            };

            var sessionService = new SessionService();
            var session = await sessionService.CreateAsync(options);

            await paymentService.CreatePaymentAsync(userId, dto.Plan ?? string.Empty, plan.amount, session.Id);

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

                    if (session.Metadata.TryGetValue("userId", out var userIdStr) &&
                        session.Metadata.TryGetValue("months", out var monthsStr))
                    {
                        await paymentService.CompleteCheckoutAsync(session.Id, int.Parse(userIdStr), int.Parse(monthsStr));
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

        [HttpGet]
        public async Task<IActionResult> GetPayments()
        {
            var payments = await paymentService.GetUserPaymentsAsync(CurrentUserId);
            return Ok(ResponseModel<List<ReturnPaymentDto>>.Ok(payments));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPayment(int id)
        {
            var payment = await paymentService.GetPaymentByIdAsync(id, CurrentUserId);
            if (payment == null)
                return NotFound(ResponseModel<string>.Fail("Payment not found."));

            return Ok(ResponseModel<ReturnPaymentDto>.Ok(payment));
        }

        [HttpGet("status")]
        public async Task<IActionResult> GetPremiumStatus()
        {
            var userId = CurrentUserId;
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