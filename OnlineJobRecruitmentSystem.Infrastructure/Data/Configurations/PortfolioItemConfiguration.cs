using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineJobRecruitmentSystem.Domain.Entities;

namespace OnlineJobRecruitmentSystem.Infrastructure.Data.Configurations
{
    public class PortfolioItemConfiguration : IEntityTypeConfiguration<PortfolioItem>
    {
        public void Configure(EntityTypeBuilder<PortfolioItem> builder)
        {
            builder.HasKey(p => p.Id);

            builder.Property(p => p.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(p => p.Description)
                .HasMaxLength(1000);

            builder.Property(p => p.FileUrl)
                .IsRequired()
                .HasMaxLength(500);

            builder.HasOne(p => p.JobSeekerProfile)
                .WithMany()
                .HasForeignKey(p => p.JobSeekerProfileId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}