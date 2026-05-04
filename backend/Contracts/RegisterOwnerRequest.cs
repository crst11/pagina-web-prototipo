namespace TiendaMicroempresas.Api.Contracts;

public sealed record RegisterOwnerRequest(
    string OwnerName,
    string BusinessName,
    string Email,
    string Password,
    string Phone,
    string City,
    string Address,
    string Tagline,
    string Description,
    string ShippingLeadTime,
    decimal MinimumOrderAmount,
    string? WebsiteUrl);
