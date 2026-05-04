namespace TiendaMicroempresas.Api.Contracts;

public sealed record BusinessCheckoutResultDto(
    int OrderId,
    int BusinessId,
    string BusinessName,
    decimal Total);
