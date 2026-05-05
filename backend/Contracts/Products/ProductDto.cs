namespace TiendaMicroempresas.Api.Contracts.Products;

public sealed record ProductDto(
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
    bool IsPublished);
