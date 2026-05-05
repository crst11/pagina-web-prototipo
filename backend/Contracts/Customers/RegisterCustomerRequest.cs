namespace TiendaMicroempresas.Api.Contracts.Customers;

public sealed record RegisterCustomerRequest(
    string FullName,
    string Email,
    string Password,
    string Phone,
    string City,
    string Address);
