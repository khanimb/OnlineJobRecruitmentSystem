using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineJobRecruitmentSystem.Domain.Entities;

namespace OnlineJobRecruitmentSystem.Infrastructure.Data.Configurations
{
    public class CvAnalysisHistoryConfiguration : IEntityTypeConfiguration<CvAnalysisHistory>
    {
        public void Configure(EntityTypeBuilder<CvAnalysisHistory> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.ResultJson)
                .IsRequired();

            builder.HasOne(c => c.JobSeekerProfile)
                .WithMany()
                .HasForeignKey(c => c.JobSeekerProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(c => c.JobPost)
                .WithMany()
                .HasForeignKey(c => c.JobPostId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
