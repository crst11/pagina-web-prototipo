using TiendaMicroempresas.Api.Repositories;

namespace TiendaMicroempresas.Api.Extensions;

/// <summary>
/// Métodos de extensión para el registro de servicios de LocalShop en el contenedor DI.
///
/// Agrupan la configuración de servicios por responsabilidad (CORS, repositorios, etc.)
/// para mantener <c>Program.cs</c> limpio y enfocado solo en el arranque de la aplicación.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registra los servicios principales de la aplicación LocalShop.
    ///
    /// Incluye:
    /// <list type="bullet">
    ///   <item>Controladores MVC (API REST)</item>
    ///   <item><see cref="IStoreRepository"/> implementado por <see cref="SqlStoreRepository"/> como Singleton,
    ///   ya que mantiene la cadena de conexión y no tiene estado mutable compartido.</item>
    /// </list>
    /// </summary>
    public static IServiceCollection AddLocalShopServices(this IServiceCollection services)
    {
        services.AddControllers();
        services.AddSingleton<IStoreRepository, SqlStoreRepository>();
        return services;
    }

    /// <summary>
    /// Configura la política CORS que permite solicitudes desde el frontend Angular.
    ///
    /// Los orígenes permitidos se leen de la sección <c>Cors:AllowedOrigins</c>
    /// en <c>appsettings.json</c>. Si no están configurados, se usa
    /// <c>http://localhost:4200</c> como valor predeterminado para desarrollo local.
    /// </summary>
    /// <param name="services">Colección de servicios del contenedor DI.</param>
    /// <param name="configuration">Configuración de la aplicación (appsettings.json).</param>
    public static IServiceCollection AddLocalShopCors(this IServiceCollection services, IConfiguration configuration)
    {
        // Lee los orígenes permitidos desde appsettings.json. Si no existe la sección,
        // usa localhost:4200 como fallback para el entorno de desarrollo.
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
