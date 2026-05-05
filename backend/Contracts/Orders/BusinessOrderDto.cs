namespace TiendaMicroempresas.Api.Contracts.Orders;

public sealed record BusinessOrderDto(
    int OrderId,
    string CustomerFullName,
    string CustomerEmail,
    string CustomerPhone,
    string CustomerCity,
    string DeliveryAddress,
    string Notes,
    string Status,
    decimal Total,
    DateTime CreatedAt,
    bool IsNew,
    string PaymentMethod,
    IReadOnlyList<BusinessOrderItemDto> Items);
