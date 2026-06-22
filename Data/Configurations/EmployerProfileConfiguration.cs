using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineJobRecruitmentSystem.Models;

namespace OnlineJobRecruitmentSystem.Data.Configurations
{
    public class EmployerProfileConfiguration : IEntityTypeConfiguration<EmployerProfile>
    {
        public void Configure(EntityTypeBuilder<EmployerProfile> builder)
        {
            builder.HasKey(e => e.Id);

            builder.Property(e => e.CompanyName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(e => e.Description)
                .HasMaxLength(1000);

            builder.Property(e => e.Website)
                .HasMaxLength(200);

            builder.Property(e => e.LogoUrl)
                .HasMaxLength(300);

            builder.HasOne(e => e.User)
                .WithOne(u => u.EmployerProfile)
                .HasForeignKey<EmployerProfile>(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
