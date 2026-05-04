namespace TiendaMicroempresas.Api.Contracts;

public sealed record CheckoutCartResponse(
    int OrderCount,
    decimal Total,
    string Message,
    IReadOnlyList<BusinessCheckoutResultDto> Orders);
