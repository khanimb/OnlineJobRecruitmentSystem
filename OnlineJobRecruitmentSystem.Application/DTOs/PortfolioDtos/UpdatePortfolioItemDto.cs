using Microsoft.AspNetCore.Http;

namespace OnlineJobRecruitmentSystem.Application.DTOs.PortfolioDtos
{
    public class UpdatePortfolioItemDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public IFormFile? File { get; set; }
    }
}