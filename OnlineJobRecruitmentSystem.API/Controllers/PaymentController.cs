using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineJobRecruitmentSystem.Application.DTOs.PaymentDtos;
using OnlineJobRecruitmentSystem.Common;
using OnlineJobRecruitmentSystem.Domain.Entities;
using OnlineJobRecruitmentSystem.Infrastructure.Data;
using System.Security.Claims;

namespace OnlineJobRecruitmentSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Employer")]
    public class PaymentController(AppDbContext context) : ControllerBase
    {
        private int GetUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        [HttpPost]
        public async Task<IActionResult> CreatePayment(CreatePaymentDto dto)
        {
            var employerId = GetUserId();
            var amount = dto.Plan == "premium" ? 99.99m : 29.99m;
            var stripePaymentId = $"stripe_{Guid.NewGuid()}";

            var payment = new Payment
            {
                EmployerId = employerId,
                StripePaymentId = stripePaymentId,
                Amount = amount,
                Status = "pending",
                Plan = dto.Plan ?? string.Empty
            };

            context.Payments.Add(payment);
            await context.SaveChangesAsync();

            return Ok(ResponseModel<ReturnPaymentDto>.Ok(new ReturnPaymentDto
            {
                Id = payment.Id,
                Plan = payment.Plan,
                Amount = payment.Amount,
                Status = payment.Status,
                StripePaymentId = payment.StripePaymentId,
                CreatedAt = payment.CreatedAt
            }, "Payment created."));
        }

        [HttpGet]
        public async Task<IActionResult> GetPayments()
        {
            var employerId = GetUserId();

            var payments = await context.Payments
                .Where(p => p.EmployerId == employerId)
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
            var employerId = GetUserId();

            var payment = await context.Payments
                .FirstOrDefaultAsync(p => p.Id == id && p.EmployerId == employerId);

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

        [HttpPut("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> UpdateStatus(int id, [FromQuery] string status)
        {
            var payment = await context.Payments.FindAsync(id);

            if (payment == null)
                return NotFound(ResponseModel<string>.Fail("Payment not found."));

            payment.Status = status;
            await context.SaveChangesAsync();

            return Ok(ResponseModel<string>.Ok(null!, "Payment status updated."));
        }
    }
}