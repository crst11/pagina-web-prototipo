using TiendaMicroempresas.Api.Contracts.Store;

namespace TiendaMicroempresas.Api.Contracts.Products;

public sealed record AdminCatalogResponse(
    MarketplaceBusinessDto Business,
    IReadOnlyList<string> Categories);
