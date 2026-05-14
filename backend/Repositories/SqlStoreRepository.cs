using System.Data;
using System.Data.Odbc;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using TiendaMicroempresas.Api.Contracts.Auth;
using TiendaMicroempresas.Api.Contracts.Customers;
using TiendaMicroempresas.Api.Contracts.Orders;
using TiendaMicroempresas.Api.Contracts.Products;
using TiendaMicroempresas.Api.Contracts.Store;

namespace TiendaMicroempresas.Api.Repositories;

/// <summary>
/// Repositorio principal de LocalShop — implementación SQL Server via ODBC.
///
/// Esta clase parcial (<c>partial</c>) está dividida en varios archivos
/// por dominio para facilitar el mantenimiento:
/// <list type="bullet">
///   <item><c>SqlStoreRepository.cs</c>       — Declaración, campo de conexión y tipos internos</item>
///   <item><c>SqlStoreRepository.Auth.cs</c>      — Autenticación y perfil empresarial</item>
///   <item><c>SqlStoreRepository.Customers.cs</c> — Autenticación y perfil de clientes</item>
///   <item><c>SqlStoreRepository.Orders.cs</c>    — Creación y checkout de pedidos</item>
///   <item><c>SqlStoreRepository.Products.cs</c>  — Gestión del catálogo de productos</item>
///   <item><c>SqlStoreRepository.Queries.cs</c>   — Consultas del marketplace y admin</item>
///   <item><c>SqlStoreRepository.Helpers.cs</c>   — Utilidades: conexión, hash, slugs, validaciones</item>
/// </list>
///
/// ## Patrón de acceso a datos
/// Usa ODBC directamente (sin ORM) para mayor control y rendimiento.
/// Todas las operaciones de escritura se ejecutan dentro de transacciones.
/// </summary>
public sealed partial class SqlStoreRepository(IConfiguration configuration) : IStoreRepository
{
    /// <summary>Cadena de conexión ODBC hacia SQL Server, leída de <c>appsettings.json</c>.</summary>
    private readonly string _connectionString = configuration.GetConnectionString("SqlServer")
        ?? throw new InvalidOperationException("No se encontro la cadena de conexion SqlServer.");

    // ─── URLs predeterminadas para imágenes ───────────────────────────────────
    // Usadas cuando el usuario no sube imágenes personalizadas.

    /// <summary>URL del logo predeterminado para empresas sin imagen propia.</summary>
    private static string DefaultLogoUrl => "/assets/images/store1.png";

    /// <summary>URL del banner predeterminado para empresas sin banner propio.</summary>
    private static string DefaultBannerUrl => "/assets/images/banner-localshop-default.jpg";

    /// <summary>URL de imagen predeterminada para productos sin imagen propia.</summary>
    private static string DefaultProductImageUrl => "/assets/images/pla1.png";


    // ─── Tipos internos: records de datos leídos de la BD ─────────────────────

    /// <summary>
    /// Datos de un producto leídos durante la validación de un pedido.
    /// Se consulta antes de confirmar la compra para verificar precio, stock y pedido mínimo.
    /// </summary>
    private sealed record ProductOrderData(
        int ProductId,
        string Name,
        decimal Price,
        int MinimumOrder,
        int Stock);


    /// <summary>
    /// Resultado de la creación de un pedido dentro de una transacción.
    /// Contiene los datos mínimos para construir la respuesta al cliente.
    /// </summary>
    private sealed record CreatedOrderResult(
        int OrderId,
        int BusinessId,
        string BusinessName,
        decimal Total);


    /// <summary>
    /// Política de pedido mínimo de una empresa, leída antes de confirmar el checkout.
    /// Permite verificar que el subtotal del carrito supere el mínimo antes de crear el pedido.
    /// </summary>
    private sealed record BusinessOrderPolicy(
        string BusinessName,
        decimal MinimumOrderAmount);


    // ─── Builders: acumuladores de filas del DataReader ───────────────────────
    // Los JOINs retornan múltiples filas por pedido (una por ítem).
    // Los builders acumulan esas filas y construyen el DTO final.

    /// <summary>
    /// Acumula los campos de un pedido de empresa mientras se leen las filas del reader.
    /// Construye un <see cref="BusinessOrderDto"/> al finalizar la lectura.
    /// </summary>
    private sealed class BusinessOrderBuilder(
        int orderId,
        string customerFullName,
        string customerEmail,
        string customerPhone,
        string customerCity,
        string deliveryAddress,
        string notes,
        string status,
        decimal total,
        DateTime createdAt,
        bool isNew,
        string paymentMethod)
    {
        public List<BusinessOrderItemDto> Items { get; } = [];

        public BusinessOrderDto Build() => new(
            orderId,
            customerFullName,
            customerEmail,
            customerPhone,
            customerCity,
            deliveryAddress,
            notes,
            status,
            total,
            createdAt,
            isNew,
            paymentMethod,
            Items);
    }

    // Acumula los campos de un pedido de cliente mientras se leen las filas del reader.
    private sealed class CustomerOrderBuilder(
        int orderId,
        int businessId,
        string businessName,
        string status,
        decimal total,
        DateTime createdAt,
        string deliveryAddress,
        string notes,
        string paymentMethod)
    {
        public List<BusinessOrderItemDto> Items { get; } = [];

        public CustomerOrderDto Build() => new(
            orderId,
            businessId,
            businessName,
            status,
            total,
            createdAt,
            deliveryAddress,
            notes,
            paymentMethod,
            Items);
    }

    // Acumula los campos de una empresa mientras se procesan las filas del JOIN con productos.
    private sealed class MarketplaceBusinessBuilder(
        int businessId,
        string slug,
        string ownerName,
        string businessName,
        string email,
        string phone,
        string city,
        string address,
        string tagline,
        string description,
        string shippingLeadTime,
        decimal minimumOrderAmount,
        string logoUrl,
        string bannerUrl,
        string websiteUrl)
    {
        public List<ProductDto> Products { get; } = [];

        public MarketplaceBusinessDto Build() => new(
            businessId,
            slug,
            ownerName,
            businessName,
            email,
            phone,
            city,
            address,
            tagline,
            description,
            shippingLeadTime,
            minimumOrderAmount,
            logoUrl,
            bannerUrl,
            websiteUrl,
            Products);
    }
}
