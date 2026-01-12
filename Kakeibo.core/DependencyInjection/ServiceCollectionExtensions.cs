using Kakeibo.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Kakeibo.Core.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddKakeiboCore(this IServiceCollection services)
    {
        services.AddScoped<ITransactionService, TransactionService>();
        return services;
    }
}
