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

// Clase principal del repositorio. Define el campo de conexion y los tipos
// de apoyo internos (builders y records). Los metodos publicos e implementaciones
// de consulta viven en archivos parciales separados por dominio.
public sealed partial class SqlStoreRepository(IConfiguration configuration) : IStoreRepository
{
    private readonly string _connectionString = configuration.GetConnectionString("SqlServer")
        ?? throw new InvalidOperationException("No se encontro la cadena de conexion SqlServer.");

    private static string DefaultLogoUrl => "/assets/images/store1.png";
    private static string DefaultBannerUrl => "/assets/images/banner-localshop-default.jpg";
    private static string DefaultProductImageUrl => "/assets/images/pla1.png";

    // Datos de un producto leidos durante la validacion de un pedido.
    private sealed record ProductOrderData(
        int ProductId,
        string Name,
        decimal Price,
        int MinimumOrder,
        int Stock);

    // Resultado de la creacion de un pedido dentro de una transaccion.
    private sealed record CreatedOrderResult(
        int OrderId,
        int BusinessId,
        string BusinessName,
        decimal Total);

    // Politica de pedido minimo de una empresa, leida antes de confirmar el checkout.
    private sealed record BusinessOrderPolicy(
        string BusinessName,
        decimal MinimumOrderAmount);

    // Acumula los campos de un pedido de empresa mientras se leen las filas del reader.
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
