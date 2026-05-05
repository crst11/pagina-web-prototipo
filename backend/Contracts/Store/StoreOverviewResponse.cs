using TiendaMicroempresas.Api.Contracts.Products;

namespace TiendaMicroempresas.Api.Contracts.Store;

public sealed record StoreOverviewResponse(
    IReadOnlyList<MarketplaceBusinessDto> Businesses,
    IReadOnlyList<FeaturedProductDto> FeaturedProducts,
    IReadOnlyList<string> Categories,
    int TotalBusinesses,
    int TotalProducts);
