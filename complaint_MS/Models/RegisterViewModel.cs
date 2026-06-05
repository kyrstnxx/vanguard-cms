// ============================================================
// NEIGHBORHOOD INCIDENT REPORT SYSTEM — EF CORE MODELS
// ============================================================

using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace complaint_MS.Models
{
    // --------------------------------------------------------
    // 1. APPLICATION USER (extends ASP.NET Identity)
    // --------------------------------------------------------
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Address { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<IncidentReport> Reports { get; set; } = new List<IncidentReport>();
    }

    // --------------------------------------------------------
    // 2. INCIDENT REPORT
    // --------------------------------------------------------
    public enum IncidentCategory
    {
        Infrastructure,
        Sports,
        Beautification,
        Grievances,
        PublicRelations
    }

    public enum ReportStatus
    {
        New,
        [Display(Name = "Under Review")]
        UnderReview,
        [Display(Name = "In Progress")]
        InProgress,
        Resolved,
        Rejected
    }

    public class IncidentReport
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; } = null!;

        [Required]
        public IncidentCategory Category { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        [StringLength(300)]
        public string? Location { get; set; }

        public string? PhotoUrl { get; set; }

        public ReportStatus Status { get; set; } = ReportStatus.New;

        public string? AssignedToUserId { get; set; }

        [ForeignKey("AssignedToUserId")]
        public ApplicationUser? AssignedTo { get; set; }

        public DateTime DateFiled { get; set; } = DateTime.UtcNow;

        public DateTime? DateResolved { get; set; }

        // Navigation
        public ICollection<StatusHistory> StatusHistories { get; set; } = new List<StatusHistory>();

        public bool IsDeleted { get; set; } = false;
    }

    // --------------------------------------------------------
    // 3. STATUS HISTORY (audit trail)
    // --------------------------------------------------------
    public class StatusHistory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ReportId { get; set; }

        [ForeignKey("ReportId")]
        public IncidentReport Report { get; set; } = null!;

        public ReportStatus OldStatus { get; set; }

        public ReportStatus NewStatus { get; set; }

        [StringLength(500)]
        public string? Remark { get; set; }

        [Required]
        public string ChangedByUserId { get; set; } = string.Empty;

        [ForeignKey("ChangedByUserId")]
        public ApplicationUser ChangedBy { get; set; } = null!;

        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    }

    // ============================================================
    // VIEW MODELS
    // ============================================================

    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
    }

    public class RegisterViewModel
    {
        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Address { get; set; }
    }

    public class SubmitReportViewModel
    {
        [Required]
        public IncidentCategory Category { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        [StringLength(300)]
        public string? Location { get; set; }

        public IFormFile? Photo { get; set; }
    }

    public class AdminDashboardViewModel
    {
        public List<IncidentReport> Reports { get; set; } = new();
        public string? FilterStatus { get; set; }
        public string? FilterCategory { get; set; }
        public int TotalNew { get; set; }
        public int TotalInProgress { get; set; }
        public int TotalResolved { get; set; }

    }
}
