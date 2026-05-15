using TiendaMicroempresas.Api.Repositories;

namespace TiendaMicroempresas.Api.Extensions;

public static class ServiceCollectionExtensions
{

    public static IServiceCollection AddLocalShopServices(this IServiceCollection services)
    {
        services.AddControllers();
        services.AddSingleton<IStoreRepository, SqlStoreRepository>();
        return services;
    }

    public static IServiceCollection AddLocalShopCors(this IServiceCollection services, IConfiguration configuration)
    {

        var allowedOrigins = configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>() ?? ["http://localhost:4200"];

        services.AddCors(options =>
        {
            options.AddPolicy("frontend", policy =>
            {
                policy.WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        return services;
    }
}
