namespace TiendaMicroempresas.Api.Contracts.Orders;

public sealed record OrderItemRequest(
    int ProductId,
    int Quantity);
