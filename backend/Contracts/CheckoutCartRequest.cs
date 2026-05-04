namespace TiendaMicroempresas.Api.Contracts;

public sealed record CheckoutCartRequest(
    string FullName,
    string Email,
    string Phone,
    string City,
    string Address,
    string? Notes,
    IReadOnlyList<CheckoutCartItemRequest> Items);
