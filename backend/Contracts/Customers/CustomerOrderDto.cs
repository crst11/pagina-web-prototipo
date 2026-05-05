using TiendaMicroempresas.Api.Contracts.Orders;

namespace TiendaMicroempresas.Api.Contracts.Customers;

public sealed record CustomerOrderDto(
    int OrderId,
    int BusinessId,
    string BusinessName,
    string Status,
    decimal Total,
    DateTime CreatedAt,
    string DeliveryAddress,
    string Notes,
    string PaymentMethod,
    IReadOnlyList<BusinessOrderItemDto> Items);
