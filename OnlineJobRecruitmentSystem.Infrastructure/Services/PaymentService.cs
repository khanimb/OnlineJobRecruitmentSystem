using AutoMapper;
using AutoMapper.QueryableExtensions;
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
        private readonly IMapper _mapper;

        public PaymentService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
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
                .ProjectTo<ReturnPaymentDto>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        public async Task<ReturnPaymentDto?> GetPaymentByIdAsync(int paymentId, int employerId)
        {
            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.Id == paymentId && p.EmployerId == employerId);

            return payment == null ? null : _mapper.Map<ReturnPaymentDto>(payment);
        }
    }
}