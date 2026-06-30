using Microsoft.EntityFrameworkCore;
using OnlineJobRecruitmentSystem.Application.DTOs.PaymentDtos;
using OnlineJobRecruitmentSystem.Application.Interfaces;
using OnlineJobRecruitmentSystem.Domain.Entities;
using OnlineJobRecruitmentSystem.Infrastructure.Data;

namespace OnlineJobRecruitmentSystem.Infrastructure.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly AppDbContext _context;

        public PaymentService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ReturnPaymentDto> CreatePaymentAsync(int employerId, CreatePaymentDto dto, string stripePaymentId)
        {
            var amount = dto.Plan == "premium" ? 99.99m : 29.99m;

            var payment = new Payment
            {
                EmployerId = employerId,
                StripePaymentId = stripePaymentId,
                Amount = amount,
                Status = "pending",
                Plan = dto.Plan ?? string.Empty
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            return MapToDto(payment);
        }

        public async Task<List<ReturnPaymentDto>> GetUserPaymentsAsync(int employerId)
        {
            return await _context.Payments
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
        }

        public async Task UpdatePaymentStatusAsync(string stripePaymentId, string status)
        {
            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.StripePaymentId == stripePaymentId);

            if (payment != null)
            {
                payment.Status = status;
                await _context.SaveChangesAsync();
            }
        }

        private static ReturnPaymentDto MapToDto(Payment payment) => new ReturnPaymentDto
        {
            Id = payment.Id,
            Plan = payment.Plan,
            Amount = payment.Amount,
            Status = payment.Status,
            StripePaymentId = payment.StripePaymentId,
            CreatedAt = payment.CreatedAt
        };
    }
}