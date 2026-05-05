// no additional usings needed
namespace TiendaMicroempresas.Api.Contracts.Customers;

public sealed record CustomerOrdersHistoryResponse(
    int OrderCount,
    decimal TotalSpent,
    IReadOnlyList<CustomerOrderDto> Orders);
