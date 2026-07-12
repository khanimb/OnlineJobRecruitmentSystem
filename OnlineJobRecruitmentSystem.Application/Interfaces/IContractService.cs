using OnlineJobRecruitmentSystem.Application.DTOs.ContractDtos;

namespace OnlineJobRecruitmentSystem.Application.Interfaces
{

    public class ContractOperationResult
    {
        public ContractResultStatus Status { get; set; }
        public ReturnContractDto? Contract { get; set; }
    }

    public class ContractAccessResult
    {
        public ContractAccessStatus Status { get; set; }
        public ReturnContractDto? Contract { get; set; }
    }

    public class ContractListResult
    {
        public bool ProfileNotFound { get; set; }
        public List<ReturnContractDto> Contracts { get; set; } = new();
    }

    public interface IContractService
    {
        Task<ContractOperationResult> CreateContractAsync(int userId, CreateContractDto dto);
        Task<ContractListResult> GetMyContractsAsync(int userId, string? role);
        Task<ContractAccessResult> GetContractAsync(int id, int userId);
        Task<ContractUpdateStatus> UpdateContractStatusAsync(int id, int userId, UpdateContractDto dto);
    }
}