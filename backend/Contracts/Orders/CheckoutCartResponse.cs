namespace TiendaMicroempresas.Api.Contracts.Orders;

public sealed record CheckoutCartResponse(
    int OrderCount,
    decimal Total,
    string Message,
    IReadOnlyList<BusinessCheckoutResultDto> Orders);
