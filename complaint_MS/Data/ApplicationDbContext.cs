// ============================================================
// ApplicationDbContext.cs
// ============================================================

using complaint_MS.Models;
using complaint_MS.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace complaint_MS.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<IncidentReport> IncidentReports { get; set; }
        public DbSet<StatusHistory> StatusHistories { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Prevent cascading delete conflicts on AssignedTo
            builder.Entity<IncidentReport>()
                .HasOne(r => r.AssignedTo)
                .WithMany()
                .HasForeignKey(r => r.AssignedToUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<IncidentReport>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reports)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<StatusHistory>()
                .HasOne(sh => sh.ChangedBy)
                .WithMany()
                .HasForeignKey(sh => sh.ChangedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
