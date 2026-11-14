using Microsoft.Extensions.DependencyInjection;
using GestionCommerciale.Application.Interfaces;
using GestionCommerciale.Application.Services;

namespace GestionCommerciale.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IClientService, ClientService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}
