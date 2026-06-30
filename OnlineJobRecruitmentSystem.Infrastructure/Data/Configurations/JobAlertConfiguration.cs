using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineJobRecruitmentSystem.Domain.Entities;

namespace OnlineJobRecruitmentSystem.Infrastructure.Data.Configurations
{
    public class JobAlertConfiguration : IEntityTypeConfiguration<JobAlert>
    {
        public void Configure(EntityTypeBuilder<JobAlert> builder)
        {
            builder.HasKey(j => j.Id);

            builder.Property(j => j.Keyword)
                .HasMaxLength(100);

            builder.Property(j => j.Location)
                .HasMaxLength(100);

            builder.Property(j => j.Frequency)
                .IsRequired()
                .HasMaxLength(10);

            builder.HasOne(j => j.User)
                .WithMany()
                .HasForeignKey(j => j.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
