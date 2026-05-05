namespace TiendaMicroempresas.Api.Contracts.Customers;

public sealed record CustomerAuthResponse(
    string Token,
    CustomerDto Customer);
