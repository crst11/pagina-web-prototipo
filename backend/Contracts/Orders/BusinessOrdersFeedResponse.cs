namespace TiendaMicroempresas.Api.Contracts.Orders;

public sealed record BusinessOrdersFeedResponse(
    int NewOrders,
    IReadOnlyList<BusinessOrderDto> Orders);
