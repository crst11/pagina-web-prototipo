using System.Data;
using Npgsql;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using TiendaMicroempresas.Api.Contracts.Auth;
using TiendaMicroempresas.Api.Contracts.Customers;
using TiendaMicroempresas.Api.Contracts.Orders;
using TiendaMicroempresas.Api.Contracts.Products;
using TiendaMicroempresas.Api.Contracts.Store;

namespace TiendaMicroempresas.Api.Repositories;

public sealed partial class SqlStoreRepository(IConfiguration configuration) : IStoreRepository
{

    private readonly string _connectionString = configuration.GetConnectionString("Supabase")
        ?? throw new InvalidOperationException("No se encontro la cadena de conexion Supabase.");

    private static string DefaultLogoUrl => "/assets/images/store1.png";

    private static string DefaultBannerUrl => "/assets/images/banner-localshop-default.jpg";

    private static string DefaultProductImageUrl => "/assets/images/pla1.png";

    private sealed record ProductOrderData(
        int ProductId,
        string Name,
        decimal Price,
        int MinimumOrder,
        int Stock);

    private sealed record CreatedOrderResult(
        int OrderId,
        int BusinessId,
        string BusinessName,
        decimal Total);

    private sealed record BusinessOrderPolicy(
        string BusinessName,
        decimal MinimumOrderAmount);

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
