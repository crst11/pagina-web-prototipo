namespace TiendaMicroempresas.Api.Contracts;

public sealed record LoginOwnerRequest(
    string Email,
    string Password);
