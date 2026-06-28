using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineJobRecruitmentSystem.Domain.Entities;

namespace OnlineJobRecruitmentSystem.Data.Configurations
{
    public class JobPostConfiguration : IEntityTypeConfiguration<JobPost>
    {
        public void Configure(EntityTypeBuilder<JobPost> builder)
        {
            builder.HasKey(j => j.Id);

            builder.Property(j => j.Title)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(j => j.Description)
                .IsRequired()
                .HasMaxLength(3000);

            builder.Property(j => j.Requirements)
                .HasMaxLength(2000);

            builder.Property(j => j.Location)
                .HasMaxLength(100);

            builder.Property(j => j.JobType)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(j => j.Category)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(j => j.SalaryMin)
                .HasColumnType("decimal(18,2)");

            builder.Property(j => j.SalaryMax)
                .HasColumnType("decimal(18,2)");

            builder.HasOne(j => j.EmployerProfile)
                .WithMany(e => e.JobPosts)
                .HasForeignKey(j => j.EmployerProfileId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
