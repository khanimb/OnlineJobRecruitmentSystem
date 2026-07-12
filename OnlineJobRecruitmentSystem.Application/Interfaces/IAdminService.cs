using OnlineJobRecruitmentSystem.Application.DTOs.AdminDtos;

namespace OnlineJobRecruitmentSystem.Application.Interfaces
{
    public enum AdminRoleUpdateResult { Success, UserNotFound, InvalidRole }

    public interface IAdminService
    {
        Task<List<AdminUserDto>> GetUsersAsync();
        Task<AdminUserDto?> GetUserAsync(int id);
        Task<bool> DeleteUserAsync(int id);
        Task<List<AdminJobDto>> GetJobsAsync();
        Task<bool> DeleteJobAsync(int id);
        Task<List<AdminApplicationDto>> GetApplicationsAsync();
        Task<List<AdminReviewDto>> GetReviewsAsync();
        Task<bool> DeleteReviewAsync(int id);
        Task<List<AdminPaymentDto>> GetPaymentsAsync();
        Task<AdminStatsDto> GetStatsAsync();
        Task<List<AdminContractDto>> GetContractsAsync();
        Task<AdminRoleUpdateResult> UpdateUserRoleAsync(int id, string role);
        Task<bool> UpdateJobStatusAsync(int id, bool isActive);
    }
}