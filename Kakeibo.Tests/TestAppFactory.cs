using Kakeibo.Core.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Kakeibo.Tests;

public sealed class TestAppFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName;

    public TestAppFactory(string? databaseName = null)
    {
        _databaseName = databaseName ?? $"kakeibo-tests-{Guid.NewGuid():N}";
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Replace SQL Server DbContext with EF InMemory for tests.
            var toRemove = services
                .Where(d => d.ServiceType == typeof(DbContextOptions<KakeiboDbContext>))
                .ToList();

            foreach (var descriptor in toRemove)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<KakeiboDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName));

            // Ensure the in-memory DB is created.
            using var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<KakeiboDbContext>();
            db.Database.EnsureCreated();
        });
    }
}
