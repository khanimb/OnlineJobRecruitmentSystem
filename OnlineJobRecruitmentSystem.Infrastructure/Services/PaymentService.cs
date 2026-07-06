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

        public async Task<PaymentPremium> CreatePaymentAsync(int employerId, string plan, decimal amount, string stripePaymentId)
        {
            var payment = new PaymentPremium
            {
                EmployerId = employerId,
                StripePaymentId = stripePaymentId,
                Amount = amount,
                Status = "pending",
                Plan = plan
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();
            return payment;
        }

        public async Task CompleteCheckoutAsync(string stripePaymentId, int userId, int months)
        {
            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.StripePaymentId == stripePaymentId);

            if (payment == null) return;

            payment.Status = "completed";

            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.IsPremium = true;
                user.PremiumExpiryDate = DateTime.UtcNow.AddMonths(months);
            }

            await _context.SaveChangesAsync();
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

        public async Task<ReturnPaymentDto?> GetPaymentByIdAsync(int paymentId, int employerId)
        {
            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.Id == paymentId && p.EmployerId == employerId);

            if (payment == null) return null;

            return new ReturnPaymentDto
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
}