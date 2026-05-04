using TiendaMicroempresas.Api.Contracts;

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

    Task<AdminCatalogResponse> GetAdminCatalogAsync(string token, CancellationToken cancellationToken);

    Task<ProductDto> CreateProductAsync(string token, UpsertProductRequest request, CancellationToken cancellationToken);

    Task<ProductDto> UpdateProductAsync(string token, int productId, UpsertProductRequest request, CancellationToken cancellationToken);

    Task DeleteProductAsync(string token, int productId, CancellationToken cancellationToken);

    Task<OrderCreatedResponse> CreateOrderAsync(CreateOrderRequest request, CancellationToken cancellationToken);

    Task<CheckoutCartResponse> CheckoutCartAsync(CheckoutCartRequest request, CancellationToken cancellationToken);

    Task<BusinessOrdersFeedResponse> GetBusinessOrdersAsync(string token, CancellationToken cancellationToken);
}
