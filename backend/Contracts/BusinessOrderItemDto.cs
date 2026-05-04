namespace TiendaMicroempresas.Api.Contracts;

public sealed record BusinessOrderItemDto(
    int ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal);
