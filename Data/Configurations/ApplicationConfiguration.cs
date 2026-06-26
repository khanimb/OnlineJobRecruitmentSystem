using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineJobRecruitmentSystem.Models;

namespace OnlineJobRecruitmentSystem.Data.Configurations
{
    public class ApplicationConfiguration : IEntityTypeConfiguration<Application>
    {
        public void Configure(EntityTypeBuilder<Application> builder)
        {
            builder.HasKey(a => a.Id);

            builder.Property(a => a.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20)
                .HasDefaultValue(ApplicationStatus.Applied);

            builder.Property(a => a.CoverLetter)
                .HasMaxLength(2000);

            builder.Property(a => a.Notes)
                .HasMaxLength(1000);

            builder.HasOne(a => a.JobPost)
                .WithMany(j => j.Applications)
                .HasForeignKey(a => a.JobPostId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(a => a.JobSeekerProfile)
                .WithMany(j => j.Applications)
                .HasForeignKey(a => a.JobSeekerProfileId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
