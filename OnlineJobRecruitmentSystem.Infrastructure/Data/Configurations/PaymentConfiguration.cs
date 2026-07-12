using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineJobRecruitmentSystem.Domain.Entities;

namespace OnlineJobRecruitmentSystem.Infrastructure.Data.Configurations
{
    public class PaymentConfiguration : IEntityTypeConfiguration<PaymentPremium>
    {
        public void Configure(EntityTypeBuilder<PaymentPremium> builder)
        {
            builder.HasKey(p => p.Id);

            builder.Property(p => p.StripePaymentId)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(p => p.Amount)
                .HasColumnType("decimal(18,2)");

            builder.Property(p => p.Status)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(p => p.Plan)
                .IsRequired()
                .HasMaxLength(20);

            builder.HasOne(p => p.Employer)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}