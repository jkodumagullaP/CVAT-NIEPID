using CAT.AID.Web.Data;
using CAT.AID.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// -----------------------------
// QuestPDF License
// -----------------------------
QuestPDF.Settings.License = LicenseType.Community;

// -----------------------------
// EPPlus License
// -----------------------------
ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

// -----------------------------
// Database Configuration
// -----------------------------
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        o => o.EnableRetryOnFailure()
    ));

// -----------------------------
// Identity Configuration
// -----------------------------
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// -----------------------------
// MVC + Razor
// -----------------------------
builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();

// -----------------------------
// Cookie Settings
// -----------------------------
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

var app = builder.Build();


// =======================================================
// 🔥 DATABASE MIGRATION + SEEDING (CRITICAL PART)
// =======================================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var context = services.GetRequiredService<ApplicationDbContext>();

    // ✅ Ensure database & tables are created
    context.Database.Migrate();

    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    // ✅ Seed roles & default admin
    await SeedData.InitializeAsync(userManager, roleManager);
}
// =======================================================


// -----------------------------
// Production Settings
// -----------------------------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// -----------------------------
// Middleware Pipeline
// -----------------------------
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// -----------------------------
// Routing
// -----------------------------
app.MapRazorPages();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();
