namespace OnlineJobRecruitmentSystem.Application.DTOs.PortfolioDtos
{
    public class ReturnPortfolioItemDto
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? FileUrl { get; set; }
        public string? FileType { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}