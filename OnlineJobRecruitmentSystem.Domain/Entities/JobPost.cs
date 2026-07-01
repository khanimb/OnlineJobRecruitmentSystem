using OnlineJobRecruitmentSystem.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineJobRecruitmentSystem.Domain.Entities
{
    public class JobPost : BaseEntity
    {
        public int EmployerProfileId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Requirements { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string JobType { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        [Column(TypeName = "decimal(18,2)")]
        public decimal SalaryMin { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal SalaryMax { get; set; }
        public DateTime Deadline { get; set; }
        public bool IsActive { get; set; } = true;
        public PaymentType? PaymentType { get; set; }
        public decimal? Budget { get; set; }

        public EmployerProfile? EmployerProfile { get; set; }
        public ICollection<JobApplication> Applications { get; set; } = new List<JobApplication>();
        public ICollection<SavedJob> SavedJobs { get; set; } = new List<SavedJob>();
    }
}
