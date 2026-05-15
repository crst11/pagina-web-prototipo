using TiendaMicroempresas.Api.Contracts.Auth;
using TiendaMicroempresas.Api.Contracts.Customers;
using TiendaMicroempresas.Api.Contracts.Orders;
using TiendaMicroempresas.Api.Contracts.Products;
using TiendaMicroempresas.Api.Contracts.Store;

namespace TiendaMicroempresas.Api.Repositories;

public interface IStoreRepository
{

    Task<bool> HasBusinessesAsync(CancellationToken cancellationToken);

    Task<StoreOverviewResponse> GetMarketplaceOverviewAsync(CancellationToken cancellationToken);

    Task<AuthResponse> RegisterOwnerAsync(RegisterOwnerRequest request, CancellationToken cancellationToken);

    Task<AuthResponse> LoginOwnerAsync(LoginOwnerRequest request, CancellationToken cancellationToken);

    Task<MarketplaceBusinessDto> GetBusinessByTokenAsync(string token, CancellationToken cancellationToken);

    Task<MarketplaceBusinessDto> UpdateOwnerProfileAsync(string token, UpdateOwnerProfileRequest request, CancellationToken cancellationToken);

    Task LogoutAsync(string token, CancellationToken cancellationToken);

    Task DeleteBusinessAsync(string token, CancellationToken cancellationToken);

    Task<CustomerAuthResponse> RegisterCustomerAsync(RegisterCustomerRequest request, CancellationToken cancellationToken);

    Task<CustomerAuthResponse> LoginCustomerAsync(LoginCustomerRequest request, CancellationToken cancellationToken);

    Task<CustomerDto> GetCustomerByTokenAsync(string token, CancellationToken cancellationToken);

    Task<CustomerDto> UpdateCustomerProfileAsync(string token, UpdateCustomerProfileRequest request, CancellationToken cancellationToken);

    Task DeleteCustomerProfileAsync(string token, CancellationToken cancellationToken);

    Task LogoutCustomerAsync(string token, CancellationToken cancellationToken);

    Task<CustomerOrdersHistoryResponse> GetCustomerOrdersAsync(string token, CancellationToken cancellationToken);

    Task<AdminCatalogResponse> GetAdminCatalogAsync(string token, CancellationToken cancellationToken);

    Task<ProductDto> CreateProductAsync(string token, UpsertProductRequest request, CancellationToken cancellationToken);

    Task<ProductDto> UpdateProductAsync(string token, int productId, UpsertProductRequest request, CancellationToken cancellationToken);

    Task DeleteProductAsync(string token, int productId, CancellationToken cancellationToken);

    Task<OrderCreatedResponse> CreateOrderAsync(CreateOrderRequest request, string customerToken, CancellationToken cancellationToken);

    Task<CheckoutCartResponse> CheckoutCartAsync(CheckoutCartRequest request, string? customerToken, CancellationToken cancellationToken);

    Task<BusinessOrdersFeedResponse> GetBusinessOrdersAsync(string token, CancellationToken cancellationToken);
}

