namespace TiendaMicroempresas.Api.Contracts;

public sealed record MarketplaceBusinessDto(
    int BusinessId,
    string Slug,
    string OwnerName,
    string BusinessName,
    string Email,
    string Phone,
    string City,
    string Address,
    string Tagline,
    string Description,
    string ShippingLeadTime,
    decimal MinimumOrderAmount,
    string LogoUrl,
    string BannerUrl,
    string WebsiteUrl,
    IReadOnlyList<ProductDto> Products);
