namespace TiendaMicroempresas.Api.Contracts.Products;

public sealed record UpsertProductRequest(
    string Category,
    string Name,
    string Description,
    decimal Price,
    int MinimumOrder,
    int Stock,
    string ImageUrl,
    bool IsFeatured,
    bool IsPublished);
