namespace TiendaMicroempresas.Api.Contracts.Auth;

public sealed record UpdateOwnerProfileRequest(
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
    string? WebsiteUrl);
