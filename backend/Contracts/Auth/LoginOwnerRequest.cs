namespace TiendaMicroempresas.Api.Contracts.Auth;

public sealed record LoginOwnerRequest(
    string Email,
    string Password);
