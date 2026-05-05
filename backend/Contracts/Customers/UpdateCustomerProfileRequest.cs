namespace TiendaMicroempresas.Api.Contracts.Customers;

public sealed record UpdateCustomerProfileRequest(
    string FullName,
    string Phone,
    string City,
    string Address);
