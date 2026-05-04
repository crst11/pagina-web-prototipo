namespace TiendaMicroempresas.Api.Contracts;

public sealed record AuthResponse(
    string Token,
    MarketplaceBusinessDto Business);
