using Aras.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Aras.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICustomerImportService, CustomerImportService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IOrderGateway, OrderGateway>();
        services.AddScoped<IWalletService, WalletService>();
        services.AddScoped<IWalletJob, WalletJob>();

        return services;
    }
}
