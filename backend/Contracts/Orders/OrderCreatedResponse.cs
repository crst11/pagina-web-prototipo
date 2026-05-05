namespace TiendaMicroempresas.Api.Contracts.Orders;

public sealed record OrderCreatedResponse(
    int OrderId,
    decimal Total,
    string Message);
