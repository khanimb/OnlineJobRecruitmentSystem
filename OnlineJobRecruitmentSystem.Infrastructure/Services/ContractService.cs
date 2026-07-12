using Microsoft.EntityFrameworkCore;
using OnlineJobRecruitmentSystem.Application.DTOs.ContractDtos;
using OnlineJobRecruitmentSystem.Application.Interfaces;
using OnlineJobRecruitmentSystem.Domain.Entities;
using OnlineJobRecruitmentSystem.Domain.Enums;
using OnlineJobRecruitmentSystem.Infrastructure.Data;

namespace OnlineJobRecruitmentSystem.Infrastructure.Services
{
    public class ContractService : IContractService
    {
        private readonly AppDbContext _context;

        public ContractService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ContractOperationResult> CreateContractAsync(int userId, CreateContractDto dto)
        {
            var employer = await _context.EmployerProfiles.FirstOrDefaultAsync(e => e.UserId == userId);
            if (employer == null)
                return new ContractOperationResult { Status = ContractResultStatus.EmployerNotFound };

            var job = await _context.JobPosts.FirstOrDefaultAsync(j => j.Id == dto.JobPostId && j.EmployerProfileId == employer.Id);
            if (job == null)
                return new ContractOperationResult { Status = ContractResultStatus.JobNotFound };

            var contract = new Contract
            {
                JobPostId = dto.JobPostId,
                EmployerProfileId = employer.Id,
                JobSeekerProfileId = dto.JobSeekerProfileId,
                Amount = dto.Amount,
                PaymentType = job.PaymentType ?? PaymentType.Fixed,
                Status = ContractStatus.Active
            };

            _context.Contracts.Add(contract);
            await _context.SaveChangesAsync();

            var jobSeeker = await _context.JobSeekerProfiles
                .Include(j => j.User)
                .FirstOrDefaultAsync(j => j.Id == dto.JobSeekerProfileId);

            var resultDto = new ReturnContractDto
            {
                Id = contract.Id,
                JobPostId = contract.JobPostId,
                JobTitle = job.Title,
                JobSeekerProfileId = contract.JobSeekerProfileId,
                JobSeekerName = jobSeeker?.FullName,
                JobSeekerUserId = jobSeeker?.UserId ?? 0,
                Amount = contract.Amount,
                Status = contract.Status,
                PaymentType = contract.PaymentType,
                CreatedAt = contract.CreatedAt
            };

            return new ContractOperationResult { Status = ContractResultStatus.Success, Contract = resultDto };
        }

        public async Task<ContractListResult> GetMyContractsAsync(int userId, string? role)
        {
            List<ReturnContractDto> contracts;

            if (role == "Employer")
            {
                var employer = await _context.EmployerProfiles.FirstOrDefaultAsync(e => e.UserId == userId);
                if (employer == null) return new ContractListResult { ProfileNotFound = true };

                contracts = await _context.Contracts
                    .Include(c => c.JobPost)
                    .Include(c => c.JobSeekerProfile)
                    .Where(c => c.EmployerProfileId == employer.Id)
                    .Select(c => new ReturnContractDto
                    {
                        Id = c.Id,
                        JobPostId = c.JobPostId,
                        JobTitle = c.JobPost.Title,
                        JobSeekerProfileId = c.JobSeekerProfileId,
                        JobSeekerName = c.JobSeekerProfile.FullName,
                        JobSeekerUserId = c.JobSeekerProfile.UserId,
                        Amount = c.Amount,
                        Status = c.Status,
                        PaymentType = c.PaymentType,
                        CompletedAt = c.CompletedAt,
                        CreatedAt = c.CreatedAt
                    })
                    .ToListAsync();
            }
            else
            {
                var jobSeeker = await _context.JobSeekerProfiles.FirstOrDefaultAsync(j => j.UserId == userId);
                if (jobSeeker == null) return new ContractListResult { ProfileNotFound = true };

                contracts = await _context.Contracts
                    .Include(c => c.JobPost)
                    .Include(c => c.EmployerProfile)
                    .Where(c => c.JobSeekerProfileId == jobSeeker.Id)
                    .Select(c => new ReturnContractDto
                    {
                        Id = c.Id,
                        JobPostId = c.JobPostId,
                        JobTitle = c.JobPost.Title,
                        JobSeekerProfileId = c.JobSeekerProfileId,
                        EmployerUserId = c.EmployerProfile.UserId,
                        EmployerName = c.EmployerProfile.CompanyName,
                        Amount = c.Amount,
                        Status = c.Status,
                        PaymentType = c.PaymentType,
                        CompletedAt = c.CompletedAt,
                        CreatedAt = c.CreatedAt
                    })
                    .ToListAsync();
            }

            var contractIds = contracts.Select(c => c.Id).ToList();
            var paidContractIds = await _context.ContractPayments
                .Where(cp => contractIds.Contains(cp.ContractId) && cp.Status == ContractPaymentStatus.Completed)
                .Select(cp => cp.ContractId)
                .ToListAsync();

            foreach (var c in contracts)
                c.IsPaid = paidContractIds.Contains(c.Id);

            return new ContractListResult { Contracts = contracts };
        }

        public async Task<ContractAccessResult> GetContractAsync(int id, int userId)
        {
            var contract = await _context.Contracts
                .Include(c => c.JobPost)
                .Include(c => c.JobSeekerProfile)
                .Include(c => c.EmployerProfile)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (contract == null)
                return new ContractAccessResult { Status = ContractAccessStatus.NotFound };

            if (contract.EmployerProfile.UserId != userId && contract.JobSeekerProfile.UserId != userId)
                return new ContractAccessResult { Status = ContractAccessStatus.Forbidden };

            return new ContractAccessResult
            {
                Status = ContractAccessStatus.Success,
                Contract = new ReturnContractDto
                {
                    Id = contract.Id,
                    JobPostId = contract.JobPostId,
                    JobTitle = contract.JobPost.Title,
                    JobSeekerProfileId = contract.JobSeekerProfileId,
                    JobSeekerName = contract.JobSeekerProfile.FullName,
                    Amount = contract.Amount,
                    Status = contract.Status,
                    PaymentType = contract.PaymentType,
                    CompletedAt = contract.CompletedAt,
                    CreatedAt = contract.CreatedAt
                }
            };
        }

        public async Task<ContractUpdateStatus> UpdateContractStatusAsync(int id, int userId, UpdateContractDto dto)
        {
            var employer = await _context.EmployerProfiles.FirstOrDefaultAsync(e => e.UserId == userId);
            if (employer == null) return ContractUpdateStatus.EmployerNotFound;

            var contract = await _context.Contracts
                .FirstOrDefaultAsync(c => c.Id == id && c.EmployerProfileId == employer.Id);

            if (contract == null) return ContractUpdateStatus.ContractNotFound;

            contract.Status = dto.Status;

            if (dto.Status == ContractStatus.Completed)
                contract.CompletedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return ContractUpdateStatus.Success;
        }
    }
}