using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineJobRecruitmentSystem.Domain.Entities;

namespace OnlineJobRecruitmentSystem.Infrastructure.Data.Configurations
{
    public class ContractPaymentConfiguration : IEntityTypeConfiguration<ContractPayment>
    {
        public void Configure(EntityTypeBuilder<ContractPayment> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.TotalAmount)
                .HasColumnType("decimal(18,2)");

            builder.Property(c => c.PlatformFee)
                .HasColumnType("decimal(18,2)");

            builder.Property(c => c.JobSeekerAmount)
                .HasColumnType("decimal(18,2)");

            builder.Property(c => c.Status)
                .HasConversion<string>()
                .HasMaxLength(20);

            builder.HasOne(c => c.Contract)
                .WithMany()
                .HasForeignKey(c => c.ContractId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}