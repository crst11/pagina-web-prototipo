namespace TiendaMicroempresas.Api.Contracts.Products;

public sealed record FeaturedProductDto(
    int ProductId,
    int BusinessId,
    string Name,
    string Category,
    string Description,
    decimal Price,
    int MinimumOrder,
    int Stock,
    string ImageUrl,
    bool IsFeatured,
    string BusinessName,
    string BusinessSlug,
    string BusinessTagline,
    string City);
