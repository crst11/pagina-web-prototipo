namespace TiendaMicroempresas.Api.Contracts.Orders;

public sealed record BusinessCheckoutResultDto(
    int OrderId,
    int BusinessId,
    string BusinessName,
    decimal Total);
