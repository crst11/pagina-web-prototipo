namespace TiendaMicroempresas.Api.Contracts;

public sealed record CreateOrderRequest(
    string FullName,
    string Email,
    string Phone,
    string City,
    string Address,
    int BusinessId,
    string? Notes,
    IReadOnlyList<OrderItemRequest> Items);
