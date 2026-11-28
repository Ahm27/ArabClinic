using System.Globalization;

using ArabClinic.Application;
using ArabClinic.Domain.Users;
using ArabClinic.Infrastructure;
using ArabClinic.Infrastructure.Persistence;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = true;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<ArabClinicDbContext>()
    .AddDefaultTokenProviders();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.SlidingExpiration = true;
});
builder.Services.AddControllersWithViews();
builder.Services.AddHealthChecks();
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var cultures = new[] { new CultureInfo("en-US"), new CultureInfo("ar-EG") };
    options.DefaultRequestCulture = new RequestCulture("en-US");
    options.SupportedCultures = cultures;
    options.SupportedUICultures = cultures;
});

var app = builder.Build();

// ── Database: apply migrations and seed ──────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<ArabClinicDbContext>();
        if (db.Database.IsRelational())
        {
            Console.WriteLine("[Startup] Applying database migrations...");
            await db.Database.MigrateAsync();
            Console.WriteLine("[Startup] Migrations applied.");
        }
        else
        {
            await db.Database.EnsureCreatedAsync();
        }

        await DbInitializer.SeedAsync(db);
        await IdentityInitializer.SeedAsync(scope.ServiceProvider, builder.Configuration);
        Console.WriteLine("[Startup] Database seeded.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Startup] ERROR during database initialisation: {ex.Message}");
        throw;
    }
}

// ── Middleware pipeline ───────────────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseRequestLocalization();
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

Console.WriteLine("[Startup] Application ready.");
app.Run();

public partial class Program;