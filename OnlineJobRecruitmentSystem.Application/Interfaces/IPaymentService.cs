using OnlineJobRecruitmentSystem.Application.DTOs.PaymentDtos;
using OnlineJobRecruitmentSystem.Domain.Entities;

namespace OnlineJobRecruitmentSystem.Application.Interfaces
{
    public interface IPaymentService
    {
        Task<PaymentPremium> CreatePaymentAsync(int employerId, string plan, decimal amount, string stripePaymentId);
        Task CompleteCheckoutAsync(string stripePaymentId, int userId, int months);
        Task<List<ReturnPaymentDto>> GetUserPaymentsAsync(int employerId);
        Task<ReturnPaymentDto?> GetPaymentByIdAsync(int paymentId, int employerId);
    }
}