using ArabClinic.Application;
using ArabClinic.Infrastructure;
using ArabClinic.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);
builder.Services.AddControllersWithViews();
builder.Services.AddHealthChecks();

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
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

Console.WriteLine("[Startup] Application ready.");
app.Run();

public partial class Program;