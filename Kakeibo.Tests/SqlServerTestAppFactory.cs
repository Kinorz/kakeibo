using Kakeibo.Core.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Kakeibo.Tests;

public sealed class SqlServerTestAppFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Use the same environment/settings as the development host.
        // This means it uses appsettings.json (+ appsettings.Development.json) and the same LocalDB connection string.
        builder.UseEnvironment("Development");
    }
}
