namespace TiendaMicroempresas.Api.Contracts.Orders;

public sealed record CheckoutCartItemRequest(
    int BusinessId,
    int ProductId,
    int Quantity);
