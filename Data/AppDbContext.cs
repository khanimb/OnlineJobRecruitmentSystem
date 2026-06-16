using Microsoft.EntityFrameworkCore;
using OnlineJobRecruitmentSystem.Models;

namespace OnlineJobRecruitmentSystem.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<EmployerProfile> EmployerProfiles { get; set; }
        public DbSet<JobSeekerProfile> JobSeekerProfiles { get; set; }
        public DbSet<JobPost> JobPosts { get; set; }
        public DbSet<Application> Applications { get; set; }
        public DbSet<SavedJob> SavedJobs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Application>()
                .HasOne(a => a.JobSeekerProfile)
                .WithMany(j => j.Applications)
                .HasForeignKey(a => a.JobSeekerProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Application>()
                .HasOne(a => a.JobPost)
                .WithMany(j => j.Applications)
                .HasForeignKey(a => a.JobPostId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SavedJob>()
                .HasOne(s => s.JobSeekerProfile)
                .WithMany(j => j.SavedJobs)
                .HasForeignKey(s => s.JobSeekerProfileId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
