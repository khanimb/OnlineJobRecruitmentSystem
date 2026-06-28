using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineJobRecruitmentSystem.Domain.Entities;
using OnlineJobRecruitmentSystem.Domain.Enums;

namespace OnlineJobRecruitmentSystem.Infrastructure.Data.Configurations
{
    public class ApplicationConfiguration : IEntityTypeConfiguration<JobApplication>
    {
        public void Configure(EntityTypeBuilder<JobApplication> builder)
        {
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
}
