using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using GestionCommerciale.Application.Interfaces;
using GestionCommerciale.Infrastructure.Data;

namespace GestionCommerciale.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var provider = configuration["DatabaseProvider"] ?? "Sqlite";

        if (provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
        {
            var connectionString = configuration.GetConnectionString("SqlServerConnection") 
                ?? "Server=localhost\\SQLEXPRESS;Database=GestionCommercialeDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";
            
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));
        }
        else
        {
            var connectionString = configuration.GetConnectionString("SqliteConnection") 
                ?? "Data Source=GestionCommerciale.db";

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(connectionString));
        }

        services.AddScoped<IApplicationDbContext>(provider => 
            provider.GetRequiredService<ApplicationDbContext>());

        return services;
    }
}
