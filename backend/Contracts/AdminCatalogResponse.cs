namespace TiendaMicroempresas.Api.Contracts;

public sealed record AdminCatalogResponse(
    MarketplaceBusinessDto Business,
    IReadOnlyList<string> Categories);
