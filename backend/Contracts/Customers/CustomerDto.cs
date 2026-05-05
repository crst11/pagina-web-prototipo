namespace TiendaMicroempresas.Api.Contracts.Customers;

public sealed record CustomerDto(
    int CustomerId,
    string FullName,
    string Email,
    string Phone,
    string City,
    string Address,
    string AuthProvider);
