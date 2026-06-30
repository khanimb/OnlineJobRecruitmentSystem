using OnlineJobRecruitmentSystem.Application.DTOs.PaymentDtos;

namespace OnlineJobRecruitmentSystem.Application.Interfaces
{
    public interface IPaymentService
    {
        Task<ReturnPaymentDto> CreatePaymentAsync(int employerId, CreatePaymentDto dto, string stripePaymentId);
        Task<List<ReturnPaymentDto>> GetUserPaymentsAsync(int employerId);
        Task UpdatePaymentStatusAsync(string stripePaymentId, string status);
    }
}