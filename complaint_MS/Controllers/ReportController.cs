using complaint_MS.Data;
using complaint_MS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace complaint_MS.Controllers
{
    [Authorize]
    public class ReportController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public ReportController(ApplicationDbContext db, UserManager<ApplicationUser> userManager, IWebHostEnvironment env)
        {
            _db = db;
            _userManager = userManager;
            _env = env;
        }

        // GET: /Report/Submit
        public IActionResult Submit() => View(new SubmitReportViewModel());

        // POST: /Report/Submit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(SubmitReportViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            string? photoUrl = null;

            if (vm.Photo != null && vm.Photo.Length > 0)
            {
                var allowed = new[] { ".jpg", ".jpeg", ".png" };
                var ext = Path.GetExtension(vm.Photo.FileName).ToLowerInvariant();

                if (!allowed.Contains(ext))
                {
                    ModelState.AddModelError("Photo", "Only JPG and PNG files are allowed.");
                    return View(vm);
                }

                if (vm.Photo.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError("Photo", "File size must not exceed 5MB.");
                    return View(vm);
                }

                var uploadsDir = Path.Combine(_env.WebRootPath, "uploads");
                Directory.CreateDirectory(uploadsDir);
                var fileName = $"{Guid.NewGuid()}{ext}";
                var filePath = Path.Combine(uploadsDir, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await vm.Photo.CopyToAsync(stream);
                photoUrl = $"/uploads/{fileName}";
            }

            var userId = _userManager.GetUserId(User)!;

            var report = new IncidentReport
            {
                UserId = userId,
                Category = vm.Category,
                Title = vm.Title,
                Description = vm.Description,
                Location = vm.Location,
                PhotoUrl = photoUrl,
                Status = ReportStatus.New,
                DateFiled = DateTime.UtcNow,
            };

            _db.IncidentReports.Add(report);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Your complaint has been submitted successfully.";
            return RedirectToAction(nameof(MyReports));
        }

        // GET: /Report/MyReports
        public async Task<IActionResult> MyReports()
        {
            var userId = _userManager.GetUserId(User)!;
            var reports = await _db.IncidentReports
                .Where(r => r.UserId == userId && r.IsDeleted == false)
                .OrderByDescending(r => r.DateFiled)
                .ToListAsync();

            return View(reports);
        }

        // GET: /Report/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var userId = _userManager.GetUserId(User)!;
            var report = await _db.IncidentReports
                .Include(r => r.StatusHistories)
                    .ThenInclude(h => h.ChangedBy)
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

            if (report == null) return NotFound();

            return View(report);
        }

        // POST: /Report/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userManager.GetUserId(User)!;

            var report = await _db.IncidentReports
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

            if (report == null) return NotFound();

            report.IsDeleted = true;
            _db.IncidentReports.Update(report);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Your complaint has been deleted.";
            return RedirectToAction(nameof(MyReports));
        }
    }
}