using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using complaint_MS.Data;
using complaint_MS.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddControllersWithViews();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    foreach (var role in new[] { "Admin", "Resident" })
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));

    // Seed Admin
    await SeedUser(userManager, "System Admin", "admin@complaint.com", "Admin@123", "Admin");

    // Seed test resident
    await SeedUser(userManager, "Test Resident", "user01@complaint.com", "User@123", "Resident");
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

// ── Helper: seed a user + role + FullName claim ──
static async Task SeedUser(UserManager<ApplicationUser> um, string fullName, string email, string password, string role)
{
    if (await um.FindByEmailAsync(email) != null) return;

    var user = new ApplicationUser
    {
        FullName = fullName,
        UserName = email,
        Email = email,
        Address = "200 Anonas Street, Sta. Mesa, Manila",
    };

    var result = await um.CreateAsync(user, password);
    if (!result.Succeeded) return;

    await um.AddToRoleAsync(user, role);
    await um.AddClaimAsync(user, new Claim("FullName", fullName));
}
