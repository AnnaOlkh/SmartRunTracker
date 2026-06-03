using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace SmartRunTracker.Infrastructure.Demo;

public static class DemoDataSeederExtensions
{
    public static async Task SeedDemoDataAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var seeder = scope.ServiceProvider.GetRequiredService<DemoDataSeeder>();

        await seeder.SeedAsync();
    }
}