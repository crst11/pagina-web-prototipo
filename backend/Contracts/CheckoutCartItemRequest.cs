namespace TiendaMicroempresas.Api.Contracts;

public sealed record CheckoutCartItemRequest(
    int BusinessId,
    int ProductId,
    int Quantity);
