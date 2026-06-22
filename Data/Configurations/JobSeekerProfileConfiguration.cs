using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineJobRecruitmentSystem.Models;

namespace OnlineJobRecruitmentSystem.Data.Configurations
{
    public class JobSeekerProfileConfiguration : IEntityTypeConfiguration<JobSeekerProfile>
    {
        public void Configure(EntityTypeBuilder<JobSeekerProfile> builder)
        {
            builder.HasKey(j => j.Id);

            builder.Property(j => j.FullName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(j => j.Phone)
                .HasMaxLength(20);

            builder.Property(j => j.Skills)
                .HasMaxLength(1000);

            builder.Property(j => j.WorkExperience)
                .HasMaxLength(2000);

            builder.Property(j => j.CvUrl)
                .HasMaxLength(300);

            builder.HasOne(j => j.User)
                .WithOne(u => u.JobSeekerProfile)
                .HasForeignKey<JobSeekerProfile>(j => j.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
