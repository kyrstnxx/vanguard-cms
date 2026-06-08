using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using complaint_MS.Models;
using complaint_MS.Data;
using Microsoft.EntityFrameworkCore;
using complaint_MS.Constants; // Added your new Constants folder!

namespace complaint_MS.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _db;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext db)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _db = db;
        }

        // ── REGISTER ──────────────────────────────────────────
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Dashboard", "Home");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var user = new ApplicationUser
            {
                FullName = vm.FullName,
                UserName = vm.Email,
                Email = vm.Email,
                Address = vm.Address,
            };

            var result = await _userManager.CreateAsync(user, vm.Password);

            if (result.Succeeded)
            {
                // USING CONSTANTS INSTEAD OF MAGIC STRINGS!
                await _userManager.AddClaimAsync(user, new Claim(AppClaims.FullName, vm.FullName));
                await _userManager.AddToRoleAsync(user, AppRoles.Resident);

                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Dashboard", "Home");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(vm);
        }

        // ── LOGIN ──────────────────────────────────────────────
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Dashboard", "Home");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var user = await _userManager.FindByEmailAsync(vm.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
                return View(vm);
            }

            var existingClaims = await _userManager.GetClaimsAsync(user);
            if (!existingClaims.Any(c => c.Type == AppClaims.FullName))
                await _userManager.AddClaimAsync(user, new Claim(AppClaims.FullName, user.FullName));

            var result = await _signInManager.PasswordSignInAsync(vm.Email, vm.Password, vm.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                if (await _userManager.IsInRoleAsync(user, AppRoles.Admin))
                    return RedirectToAction("Dashboard", "Admin");

                return RedirectToAction("Dashboard", "Home");
            }

            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return View(vm);
        }

        // ── LOGOUT ─────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home"); // Logging out should send you to the public landing page
        }

        // ── SETTINGS (GET) ─────────────────────────────────────
        [Authorize]
        public async Task<IActionResult> Settings()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");

            var vm = new SettingsViewModel
            {
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                Address = user.Address,
            };

            return View(vm);
        }

        // ── SETTINGS (POST) ────────────────────────────────────
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Settings(SettingsViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");

            bool nameChanged = user.FullName != vm.FullName;

            user.FullName = vm.FullName;
            user.Address = vm.Address;

            if (!string.Equals(user.Email, vm.Email, StringComparison.OrdinalIgnoreCase))
            {
                user.Email = vm.Email;
                user.UserName = vm.Email;
            }

            var updateResult = await _userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
            {
                foreach (var error in updateResult.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return View(vm);
            }

            if (nameChanged)
            {
                var claims = await _userManager.GetClaimsAsync(user);
                var oldClaim = claims.FirstOrDefault(c => c.Type == AppClaims.FullName);
                if (oldClaim != null)
                    await _userManager.ReplaceClaimAsync(user, oldClaim, new Claim(AppClaims.FullName, vm.FullName));
                else
                    await _userManager.AddClaimAsync(user, new Claim(AppClaims.FullName, vm.FullName));
            }

            await _signInManager.RefreshSignInAsync(user);

            TempData["Success"] = "Your profile has been updated successfully.";
            return RedirectToAction(nameof(Settings));
        }

        // ── CHANGE PASSWORD (POST) ─────────────────────────────
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                TempData["PasswordError"] = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .FirstOrDefault()?.ErrorMessage ?? "Invalid input.";
                return RedirectToAction(nameof(Settings));
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");

            var result = await _userManager.ChangePasswordAsync(user, vm.CurrentPassword, vm.NewPassword);

            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);
                TempData["PasswordSuccess"] = "Password changed successfully.";
            }
            else
            {
                TempData["PasswordError"] = result.Errors.FirstOrDefault()?.Description ?? "Password change failed.";
            }

            return RedirectToAction(nameof(Settings));
        }

        // ── DELETE ACCOUNT (POST) ──────────────────────────────
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAccount()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");

            var reports = await _db.IncidentReports
                .Where(r => r.UserId == user.Id)
                .ToListAsync();

            foreach (var report in reports)
                report.IsDeleted = true;

            await _db.SaveChangesAsync();

            await _signInManager.SignOutAsync();
            await _userManager.DeleteAsync(user);

            return RedirectToAction("Login", new { deleted = true });
        }
    }
}