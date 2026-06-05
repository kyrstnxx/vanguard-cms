using complaint_MS.Data;
using complaint_MS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace complaint_MS.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        // GET: /Admin/Dashboard
        public async Task<IActionResult> Dashboard(string? filterStatus, string? filterCategory, string? search)
        {
            var query = _db.IncidentReports
                .Include(r => r.User)
                .Include(r => r.AssignedTo)
                .Where(r => r.IsDeleted == false)
                .AsQueryable();

            if (!string.IsNullOrEmpty(filterStatus) && Enum.TryParse<ReportStatus>(filterStatus, out var status))
                query = query.Where(r => r.Status == status);

            if (!string.IsNullOrEmpty(filterCategory) && Enum.TryParse<IncidentCategory>(filterCategory, out var cat))
                query = query.Where(r => r.Category == cat);

            if (!string.IsNullOrEmpty(search))
                query = query.Where(r => r.Title.Contains(search) || (r.Location != null && r.Location.Contains(search)));

            var vm = new AdminDashboardViewModel
            {
                Reports = await query.OrderByDescending(r => r.DateFiled).ToListAsync(),
                FilterStatus = filterStatus,
                FilterCategory = filterCategory,
                TotalNew = await _db.IncidentReports.CountAsync(r => r.Status == ReportStatus.New),
                TotalInProgress = await _db.IncidentReports.CountAsync(r => r.Status == ReportStatus.InProgress),
                TotalResolved = await _db.IncidentReports.CountAsync(r => r.Status == ReportStatus.Resolved),
            };

            return View(vm);
        }

        // GET: /Admin/ReportDetails/5
        public async Task<IActionResult> ReportDetails(int id)
        {
            var report = await _db.IncidentReports
                .Include(r => r.User)
                .Include(r => r.AssignedTo)
                .Include(r => r.StatusHistories)
                    .ThenInclude(h => h.ChangedBy)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (report == null) return NotFound();

            ViewBag.StaffList = await _userManager.GetUsersInRoleAsync("Admin");
            return View(report);
        }

        // POST: /Admin/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int reportId, ReportStatus newStatus, string? remark, string? assignedToUserId)
        {
            var report = await _db.IncidentReports.FindAsync(reportId);
            if (report == null) return NotFound();

            var adminId = _userManager.GetUserId(User)!;

            var history = new StatusHistory
            {
                ReportId = reportId,
                OldStatus = report.Status,
                NewStatus = newStatus,
                Remark = remark,
                ChangedByUserId = adminId,
                ChangedAt = DateTime.UtcNow,
            };

            report.Status = newStatus;

            if (!string.IsNullOrEmpty(assignedToUserId))
                report.AssignedToUserId = assignedToUserId;

            if (newStatus == ReportStatus.Resolved)
                report.DateResolved = DateTime.UtcNow;

            _db.StatusHistories.Add(history);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Report status updated successfully.";
            return RedirectToAction(nameof(ReportDetails), new { id = reportId });
        }

        // GET: /Admin/Residents
        public async Task<IActionResult> Residents()
        {
            // Fetch all users assigned to the "Resident" role
            var residents = await _userManager.GetUsersInRoleAsync("Resident");

            // Pass the list to the view
            return View(residents);
        }

        // POST: /Admin/DeleteReport/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteReport(int id)
        {
            var report = await _db.IncidentReports.FindAsync(id);
            if (report == null) return NotFound();

            report.IsDeleted = true;
            _db.IncidentReports.Update(report);
            await _db.SaveChangesAsync();

            TempData["Success"] = "The report was permanently deleted.";
            return RedirectToAction(nameof(Dashboard));
        }
    }
}
