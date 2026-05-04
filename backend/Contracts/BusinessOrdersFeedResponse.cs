namespace TiendaMicroempresas.Api.Contracts;

public sealed record BusinessOrdersFeedResponse(
    int NewOrders,
    IReadOnlyList<BusinessOrderDto> Orders);
