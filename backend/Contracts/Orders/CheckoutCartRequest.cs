namespace TiendaMicroempresas.Api.Contracts.Orders;

public sealed record CheckoutCartRequest(
    string FullName,
    string Email,
    string Phone,
    string City,
    string Address,
    string? Notes,
    string PaymentMethod,
    IReadOnlyList<CheckoutCartItemRequest> Items);
