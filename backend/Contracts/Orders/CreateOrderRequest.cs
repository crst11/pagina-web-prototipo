namespace TiendaMicroempresas.Api.Contracts.Orders;

public sealed record CreateOrderRequest(
    string FullName,
    string Email,
    string Phone,
    string City,
    string Address,
    int BusinessId,
    string? Notes,
    string PaymentMethod,
    IReadOnlyList<OrderItemRequest> Items);
