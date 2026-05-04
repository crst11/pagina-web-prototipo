namespace TiendaMicroempresas.Api.Contracts;

public sealed record StoreOverviewResponse(
    IReadOnlyList<MarketplaceBusinessDto> Businesses,
    IReadOnlyList<FeaturedProductDto> FeaturedProducts,
    IReadOnlyList<string> Categories,
    int TotalBusinesses,
    int TotalProducts);
