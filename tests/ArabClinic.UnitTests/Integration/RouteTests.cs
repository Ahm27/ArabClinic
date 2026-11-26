using ArabClinic.Infrastructure.Persistence;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ArabClinic.UnitTests.Integration;

public sealed class RouteTests : IClassFixture<ArabClinicWebApplicationFactory>
{
    private readonly HttpClient _client;

    public RouteTests(ArabClinicWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Theory]
    [InlineData("/")]
    [InlineData("/Doctors")]
    [InlineData("/Departments")]
    [InlineData("/Appointments")]
    [InlineData("/Appointments/Create")]
    [InlineData("/Patients")]
    [InlineData("/Patients/Create")]
    [InlineData("/Invoices")]
    [InlineData("/Notifications")]
    [InlineData("/Home/About")]
    [InlineData("/Home/Services")]
    [InlineData("/Home/Contact")]
    [InlineData("/health")]
    public async Task ExistingRoutesReturnSuccess(string route)
    {
        using var response = await _client.GetAsync(route);

        Assert.True(
            response.IsSuccessStatusCode,
            await response.Content.ReadAsStringAsync());
    }
}

public sealed class ArabClinicWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<ArabClinicDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<ArabClinicDbContext>>();
            services.RemoveAll<ArabClinicDbContext>();
            services.AddDbContext<ArabClinicDbContext>(options =>
                options.UseInMemoryDatabase($"ArabClinicTests-{Guid.NewGuid()}"));
        });
    }
}