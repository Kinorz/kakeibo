using Kakeibo.Core.Data;
using Kakeibo.Core.DependencyInjection;
using Kakeibo.Core.Handlers;
using Microsoft.EntityFrameworkCore;

namespace Kakeibo;

public sealed class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();

        // App services
        services.AddKakeiboCore();

        // Persistence (SQL Server)
        services.AddDbContext<KakeiboDbContext>(options =>
        {
            options.UseSqlServer(Configuration.GetConnectionString("Kakeibo"));

            // Development only: include parameter values in logs.
            // WARNING: Do not enable this in production.
            if (string.Equals(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"), "Development", StringComparison.OrdinalIgnoreCase))
            {
                options.EnableSensitiveDataLogging();
            }
        });
        services.AddScoped<EfTransactionHandler>();
    }

    public void Configure(WebApplication app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<KakeiboDbContext>();
            db.Database.Migrate();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();
    }
}
