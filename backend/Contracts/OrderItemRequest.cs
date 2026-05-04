namespace TiendaMicroempresas.Api.Contracts;

public sealed record OrderItemRequest(
    int ProductId,
    int Quantity);
