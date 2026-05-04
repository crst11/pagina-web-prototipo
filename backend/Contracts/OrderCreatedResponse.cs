namespace TiendaMicroempresas.Api.Contracts;

public sealed record OrderCreatedResponse(
    int OrderId,
    decimal Total,
    string Message);
