namespace TiendaMicroempresas.Api.Contracts.Customers;

public sealed record LoginCustomerRequest(
    string Email,
    string Password);
