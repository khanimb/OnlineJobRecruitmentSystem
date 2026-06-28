using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineJobRecruitmentSystem.Domain.Entities;

namespace OnlineJobRecruitmentSystem.Data.Configurations
{
    public class SavedJobConfiguration : IEntityTypeConfiguration<SavedJob>
    {
        public void Configure(EntityTypeBuilder<SavedJob> builder)
        {
            builder.HasKey(s => s.Id);

            builder.HasIndex(s => new { s.JobSeekerProfileId, s.JobPostId })
                .IsUnique();

            builder.HasOne(s => s.JobSeekerProfile)
                .WithMany(j => j.SavedJobs)
                .HasForeignKey(s => s.JobSeekerProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(s => s.JobPost)
                .WithMany(j => j.SavedJobs)
                .HasForeignKey(s => s.JobPostId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
