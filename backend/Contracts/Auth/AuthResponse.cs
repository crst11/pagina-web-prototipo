using TiendaMicroempresas.Api.Contracts.Store;

namespace TiendaMicroempresas.Api.Contracts.Auth;

public sealed record AuthResponse(
    string Token,
    MarketplaceBusinessDto Business);
