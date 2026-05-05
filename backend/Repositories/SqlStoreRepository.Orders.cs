using System.Data.Odbc;
using System.Globalization;
using TiendaMicroempresas.Api.Contracts.Orders;

namespace TiendaMicroempresas.Api.Repositories;

// Creacion de pedidos individuales y checkout del carrito completo.
// Cada operacion usa una transaccion para garantizar consistencia de inventario.
public sealed partial class SqlStoreRepository
{
    public Task<OrderCreatedResponse> CreateOrderAsync(CreateOrderRequest request, string customerToken, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var connection = CreateOpenConnection();
        using var transaction = connection.BeginTransaction();

        try
        {
            var customerId = GetCustomerIdByToken(connection, transaction, customerToken);
            var createdOrder = CreateBusinessOrder(
                connection,
                transaction,
                request.BusinessId,
                customerId,
                request.FullName,
                request.Email,
                request.Phone,
                request.City,
                request.Address,
                request.Notes,
                request.PaymentMethod,
                request.Items.Select(item => (item.ProductId, item.Quantity)).ToList());

            transaction.Commit();

            return Task.FromResult(new OrderCreatedResponse(
                createdOrder.OrderId,
                createdOrder.Total,
                $"Pedido enviado a {createdOrder.BusinessName}. El equipo comercial revisara tu solicitud."));
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public Task<CheckoutCartResponse> CheckoutCartAsync(CheckoutCartRequest request, string? customerToken, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (request.Items.Count == 0)
        {
            throw new InvalidOperationException("Debes agregar al menos un producto antes de comprar.");
        }

        using var connection = CreateOpenConnection();
        using var transaction = connection.BeginTransaction();

        try
        {
            var customerId = string.IsNullOrWhiteSpace(customerToken)
                ? (int?)null
                : GetCustomerIdByToken(connection, transaction, customerToken);

            var createdOrders = request.Items
                .GroupBy(item => item.BusinessId)
                .Select(group => CreateBusinessOrder(
                    connection,
                    transaction,
                    group.Key,
                    customerId,
                    request.FullName,
                    request.Email,
                    request.Phone,
                    request.City,
                    request.Address,
                    request.Notes,
                    request.PaymentMethod,
                    group.Select(item => (item.ProductId, item.Quantity)).ToList()))
                .ToList();

            transaction.Commit();

            return Task.FromResult(new CheckoutCartResponse(
                createdOrders.Count,
                createdOrders.Sum(order => order.Total),
                $"Se registraron {createdOrders.Count} pedidos para tus empresas seleccionadas.",
                createdOrders
                    .Select(order => new BusinessCheckoutResultDto(order.OrderId, order.BusinessId, order.BusinessName, order.Total))
                    .ToList()));
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public Task<BusinessOrdersFeedResponse> GetBusinessOrdersAsync(string token, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var connection = CreateOpenConnection();
        var businessId = GetBusinessIdByToken(connection, null, token);
        var orders = GetBusinessOrdersFeed(connection, null, businessId);
        return Task.FromResult(new BusinessOrdersFeedResponse(
            orders.Count(order => order.IsNew),
            orders));
    }

    // Crea un pedido para una sola empresa dentro de una transaccion activa.
    // Valida stock, pedido minimo y descuenta inventario por cada linea.
    private static CreatedOrderResult CreateBusinessOrder(
        OdbcConnection connection,
        OdbcTransaction transaction,
        int businessId,
        int? customerId,
        string fullName,
        string email,
        string phone,
        string city,
        string address,
        string? notes,
        string paymentMethod,
        IReadOnlyList<(int ProductId, int Quantity)> items)
    {
        var businessOrderPolicy = GetBusinessOrderPolicy(connection, transaction, businessId);
        var total = 0m;
        var orderLines = new List<(int ProductId, int Quantity, decimal UnitPrice)>();

        foreach (var item in items)
        {
            if (item.Quantity <= 0)
            {
                throw new InvalidOperationException("Todas las cantidades del pedido deben ser mayores a cero.");
            }

            var product = GetProductForOrder(connection, transaction, businessId, item.ProductId);

            if (item.Quantity > product.Stock)
            {
                throw new InvalidOperationException(
                    $"No hay inventario suficiente para {product.Name}. Stock disponible: {product.Stock}.");
            }

            total += product.Price * item.Quantity;
            orderLines.Add((item.ProductId, item.Quantity, product.Price));
        }

        if (total < businessOrderPolicy.MinimumOrderAmount)
        {
            throw new InvalidOperationException(
                $"El pedido minimo para {businessOrderPolicy.BusinessName} es de {businessOrderPolicy.MinimumOrderAmount.ToString("C0", CultureInfo.GetCultureInfo("es-CO"))}.");
        }

        var orderId = InsertAndGetId(
            connection,
            transaction,
            """
            INSERT INTO dbo.Orders
            (
                CustomerId,
                BusinessId,
                CustomerFullName,
                CustomerEmail,
                CustomerPhone,
                CustomerCity,
                DeliveryAddress,
                Notes,
                Status,
                Total,
                PaymentMethod
            )
            VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?);
            SELECT CAST(SCOPE_IDENTITY() AS INT);
            """,
            customerId,
            businessId,
            RequireText(fullName, "Ingresa el nombre completo del comprador."),
            NormalizeEmail(email),
            RequireText(phone, "Ingresa un telefono de contacto."),
            RequireText(city, "Ingresa la ciudad de entrega."),
            RequireText(address, "Ingresa la direccion de entrega."),
            NormalizeOptionalText(notes),
            "Pendiente",
            total,
            RequireText(paymentMethod, "Ingresa un metodo de pago."));

        foreach (var line in orderLines)
        {
            ExecuteNonQuery(
                connection,
                transaction,
                """
                INSERT INTO dbo.OrderItems (OrderId, ProductId, Quantity, UnitPrice, LineTotal)
                VALUES (?, ?, ?, ?, ?);
                """,
                orderId,
                line.ProductId,
                line.Quantity,
                line.UnitPrice,
                line.UnitPrice * line.Quantity);

            ExecuteNonQuery(
                connection,
                transaction,
                """
                UPDATE dbo.Products
                SET Stock = Stock - ?, UpdatedAt = SYSDATETIME()
                WHERE ProductId = ? AND IsArchived = 0;
                """,
                line.Quantity,
                line.ProductId);
        }

        return new CreatedOrderResult(orderId, businessId, businessOrderPolicy.BusinessName, total);
    }
}
