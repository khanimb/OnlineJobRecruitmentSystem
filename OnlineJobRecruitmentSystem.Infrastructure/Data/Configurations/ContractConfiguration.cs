using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineJobRecruitmentSystem.Domain.Entities;

namespace OnlineJobRecruitmentSystem.Infrastructure.Data.Configurations
{
    public class ContractConfiguration : IEntityTypeConfiguration<Contract>
    {
        public void Configure(EntityTypeBuilder<Contract> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Amount)
                .HasColumnType("decimal(18,2)");

            builder.Property(c => c.Status)
                .HasConversion<string>();

            builder.Property(c => c.PaymentType)
                .HasConversion<string>();

            builder.HasOne(c => c.JobPost)
                .WithMany()
                .HasForeignKey(c => c.JobPostId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(c => c.EmployerProfile)
                .WithMany()
                .HasForeignKey(c => c.EmployerProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(c => c.JobSeekerProfile)
                .WithMany()
                .HasForeignKey(c => c.JobSeekerProfileId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}