using System.Data;
using System.Data.Odbc;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using TiendaMicroempresas.Api.Contracts;

namespace TiendaMicroempresas.Api.Repositories;

public sealed class SqlStoreRepository(IConfiguration configuration) : IStoreRepository
{
    private readonly string _connectionString = configuration.GetConnectionString("SqlServer")
        ?? throw new InvalidOperationException("No se encontro la cadena de conexion SqlServer.");

    public Task<bool> HasBusinessesAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var connection = CreateOpenConnection();
        var count = Convert.ToInt32(ExecuteScalar(connection, null, "SELECT COUNT(*) FROM dbo.Businesses;"));
        return Task.FromResult(count > 0);
    }

    public Task<StoreOverviewResponse> GetMarketplaceOverviewAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var connection = CreateOpenConnection();
        var businesses = GetBusinesses(connection, null, publishedOnly: true, businessId: null);
        var featuredProducts = businesses
            .SelectMany(business => business.Products
                .Where(product => product.IsFeatured && product.IsPublished)
                .Select(product => new FeaturedProductDto(
                    product.ProductId,
                    product.BusinessId,
                    product.Name,
                    product.Category,
                    product.Description,
                    product.Price,
                    product.MinimumOrder,
                    product.Stock,
                    product.ImageUrl,
                    product.IsFeatured,
                    business.BusinessName,
                    business.Slug,
                    business.Tagline,
                    business.City)))
            .OrderByDescending(product => product.Stock)
            .ThenBy(product => product.BusinessName)
            .Take(8)
            .ToList();

        var categories = businesses
            .SelectMany(business => business.Products)
            .Select(product => product.Category.Trim())
            .Where(category => !string.IsNullOrWhiteSpace(category))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(category => category, StringComparer.Create(new System.Globalization.CultureInfo("es-CO"), false))
            .ToList();

        return Task.FromResult(new StoreOverviewResponse(
            businesses,
            featuredProducts,
            categories,
            businesses.Count,
            businesses.Sum(business => business.Products.Count)));
    }

    public async Task<AuthResponse> RegisterOwnerAsync(RegisterOwnerRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var connection = CreateOpenConnection();
        var normalizedEmail = NormalizeEmail(request.Email);
        EnsureEmailAvailable(connection, null, normalizedEmail, null);

        var salt = CreateSalt();
        var hash = HashPassword(request.Password, salt);
        var businessName = RequireText(request.BusinessName, "Ingresa el nombre de la empresa.");
        var businessId = InsertAndGetId(
            connection,
            null,
            """
            INSERT INTO dbo.Businesses
            (
                Slug,
                OwnerName,
                BusinessName,
                Email,
                PasswordHash,
                PasswordSalt,
                Phone,
                City,
                Address,
                Tagline,
                Description,
                ShippingLeadTime,
                MinimumOrderAmount,
                LogoUrl,
                BannerUrl,
                WebsiteUrl
            )
            VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?);
            SELECT CAST(SCOPE_IDENTITY() AS INT);
            """,
            BuildUniqueSlug(connection, null, businessName, null),
            RequireText(request.OwnerName, "Ingresa el nombre de la persona responsable."),
            businessName,
            normalizedEmail,
            hash,
            salt,
            RequireText(request.Phone, "Ingresa un telefono de contacto."),
            RequireText(request.City, "Ingresa la ciudad principal de operacion."),
            RequireText(request.Address, "Ingresa la direccion comercial."),
            RequireText(request.Tagline, "Ingresa una frase corta para presentar la empresa."),
            RequireText(request.Description, "Ingresa una descripcion clara del negocio."),
            RequireText(request.ShippingLeadTime, "Ingresa el tiempo estimado de entrega."),
            RequireMoney(request.MinimumOrderAmount, "El pedido minimo por empresa no puede ser negativo."),
            DefaultLogoUrl,
            DefaultBannerUrl,
            NormalizeOptionalText(request.WebsiteUrl));

        return await CreateSessionResponseAsync(connection, businessId);
    }

    public async Task<AuthResponse> LoginOwnerAsync(LoginOwnerRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var connection = CreateOpenConnection();
        using var command = CreateCommand(
            connection,
            null,
            """
            SELECT BusinessId, PasswordHash, PasswordSalt
            FROM dbo.Businesses
            WHERE Email = ?;
            """,
            NormalizeEmail(request.Email));

        using var reader = command.ExecuteReader(CommandBehavior.SingleRow);
        if (!reader.Read())
        {
            throw new InvalidOperationException("Correo o contrasena incorrectos.");
        }

        var businessId = reader.GetInt32(0);
        var passwordHash = reader.GetString(1);
        var passwordSalt = reader.GetString(2);

        if (!VerifyPassword(request.Password, passwordSalt, passwordHash))
        {
            throw new InvalidOperationException("Correo o contrasena incorrectos.");
        }

        return await CreateSessionResponseAsync(connection, businessId);
    }

    public Task<MarketplaceBusinessDto> GetBusinessByTokenAsync(string token, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var connection = CreateOpenConnection();
        var businessId = GetBusinessIdByToken(connection, null, token);
        return Task.FromResult(GetBusinessById(connection, null, businessId, publishedOnly: false));
    }

    public Task<MarketplaceBusinessDto> UpdateOwnerProfileAsync(string token, UpdateOwnerProfileRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var connection = CreateOpenConnection();
        var businessId = GetBusinessIdByToken(connection, null, token);
        var normalizedEmail = NormalizeEmail(request.Email);

        EnsureEmailAvailable(connection, null, normalizedEmail, businessId);

        ExecuteNonQuery(
            connection,
            null,
            """
            UPDATE dbo.Businesses
            SET Slug = ?,
                OwnerName = ?,
                BusinessName = ?,
                Email = ?,
                Phone = ?,
                City = ?,
                Address = ?,
                Tagline = ?,
                Description = ?,
                ShippingLeadTime = ?,
                MinimumOrderAmount = ?,
                LogoUrl = ?,
                BannerUrl = ?,
                WebsiteUrl = ?,
                UpdatedAt = SYSDATETIME()
            WHERE BusinessId = ?;
            """,
            BuildUniqueSlug(connection, null, request.BusinessName, businessId),
            RequireText(request.OwnerName, "Ingresa el nombre de la persona responsable."),
            RequireText(request.BusinessName, "Ingresa el nombre de la empresa."),
            normalizedEmail,
            RequireText(request.Phone, "Ingresa un telefono de contacto."),
            RequireText(request.City, "Ingresa la ciudad principal de operacion."),
            RequireText(request.Address, "Ingresa la direccion comercial."),
            RequireText(request.Tagline, "Ingresa una frase corta para presentar la empresa."),
            RequireText(request.Description, "Ingresa una descripcion clara del negocio."),
            RequireText(request.ShippingLeadTime, "Ingresa el tiempo estimado de entrega."),
            RequireMoney(request.MinimumOrderAmount, "El pedido minimo por empresa no puede ser negativo."),
            RequireImageUrl(request.LogoUrl, DefaultLogoUrl),
            RequireImageUrl(request.BannerUrl, DefaultBannerUrl),
            NormalizeOptionalText(request.WebsiteUrl),
            businessId);

        return Task.FromResult(GetBusinessById(connection, null, businessId, publishedOnly: false));
    }

    public Task LogoutAsync(string token, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var connection = CreateOpenConnection();
        ExecuteNonQuery(connection, null, "DELETE FROM dbo.BusinessSessions WHERE SessionToken = ?;", token);
        return Task.CompletedTask;
    }

    public Task<AdminCatalogResponse> GetAdminCatalogAsync(string token, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var connection = CreateOpenConnection();
        var businessId = GetBusinessIdByToken(connection, null, token);
        var business = GetBusinessById(connection, null, businessId, publishedOnly: false);
        var categories = business.Products
            .Select(product => product.Category.Trim())
            .Where(category => !string.IsNullOrWhiteSpace(category))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(category => category, StringComparer.Create(new System.Globalization.CultureInfo("es-CO"), false))
            .ToList();

        return Task.FromResult(new AdminCatalogResponse(business, categories));
    }

    public Task<ProductDto> CreateProductAsync(string token, UpsertProductRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var connection = CreateOpenConnection();
        var businessId = GetBusinessIdByToken(connection, null, token);
        var productId = InsertAndGetId(
            connection,
            null,
            """
            INSERT INTO dbo.Products
            (
                BusinessId,
                Name,
                Category,
                Description,
                Price,
                MinimumOrder,
                Stock,
                ImageUrl,
                IsFeatured,
                IsPublished
            )
            VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?);
            SELECT CAST(SCOPE_IDENTITY() AS INT);
            """,
            businessId,
            RequireText(request.Name, "Ingresa un nombre para el producto."),
            RequireText(request.Category, "Ingresa una categoria para el producto."),
            RequireText(request.Description, "Ingresa una descripcion para el producto."),
            RequireMoney(request.Price),
            RequireMinimumValue(request.MinimumOrder, 1, "El pedido minimo debe ser al menos 1."),
            RequireMinimumValue(request.Stock, 0, "El stock no puede ser negativo."),
            RequireImageUrl(request.ImageUrl, DefaultProductImageUrl),
            request.IsFeatured,
            request.IsPublished);

        return Task.FromResult(GetOwnedProductById(connection, null, businessId, productId));
    }

    public Task<ProductDto> UpdateProductAsync(string token, int productId, UpsertProductRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var connection = CreateOpenConnection();
        var businessId = GetBusinessIdByToken(connection, null, token);
        EnsureProductOwnership(connection, null, businessId, productId);

        var affectedRows = ExecuteNonQuery(
            connection,
            null,
            """
            UPDATE dbo.Products
            SET Name = ?,
                Category = ?,
                Description = ?,
                Price = ?,
                MinimumOrder = ?,
                Stock = ?,
                ImageUrl = ?,
                IsFeatured = ?,
                IsPublished = ?,
                UpdatedAt = SYSDATETIME()
            WHERE ProductId = ? AND BusinessId = ? AND IsArchived = 0;
            """,
            RequireText(request.Name, "Ingresa un nombre para el producto."),
            RequireText(request.Category, "Ingresa una categoria para el producto."),
            RequireText(request.Description, "Ingresa una descripcion para el producto."),
            RequireMoney(request.Price),
            RequireMinimumValue(request.MinimumOrder, 1, "El pedido minimo debe ser al menos 1."),
            RequireMinimumValue(request.Stock, 0, "El stock no puede ser negativo."),
            RequireImageUrl(request.ImageUrl, DefaultProductImageUrl),
            request.IsFeatured,
            request.IsPublished,
            productId,
            businessId);

        if (affectedRows == 0)
        {
            throw new InvalidOperationException("El producto no existe o no pertenece a tu empresa.");
        }

        return Task.FromResult(GetOwnedProductById(connection, null, businessId, productId));
    }

    public Task DeleteProductAsync(string token, int productId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var connection = CreateOpenConnection();
        var businessId = GetBusinessIdByToken(connection, null, token);
        EnsureProductOwnership(connection, null, businessId, productId);

        var affectedRows = ExecuteNonQuery(
            connection,
            null,
            """
            UPDATE dbo.Products
            SET IsArchived = 1,
                IsPublished = 0,
                IsFeatured = 0,
                Stock = 0,
                UpdatedAt = SYSDATETIME()
            WHERE ProductId = ? AND BusinessId = ? AND IsArchived = 0;
            """,
            productId,
            businessId);

        if (affectedRows == 0)
        {
            throw new InvalidOperationException("El producto no existe o no pertenece a tu empresa.");
        }

        return Task.CompletedTask;
    }

    public Task<OrderCreatedResponse> CreateOrderAsync(CreateOrderRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var connection = CreateOpenConnection();
        using var transaction = connection.BeginTransaction();

        try
        {
            var createdOrder = CreateBusinessOrder(
                connection,
                transaction,
                request.BusinessId,
                request.FullName,
                request.Email,
                request.Phone,
                request.City,
                request.Address,
                request.Notes,
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

    public Task<CheckoutCartResponse> CheckoutCartAsync(CheckoutCartRequest request, CancellationToken cancellationToken)
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
            var createdOrders = request.Items
                .GroupBy(item => item.BusinessId)
                .Select(group => CreateBusinessOrder(
                    connection,
                    transaction,
                    group.Key,
                    request.FullName,
                    request.Email,
                    request.Phone,
                    request.City,
                    request.Address,
                    request.Notes,
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

    private static string DefaultLogoUrl => "/assets/images/store1.png";

    private static string DefaultBannerUrl => "/assets/images/banner-localshop-default.jpg";

    private static string DefaultProductImageUrl => "/assets/images/pla1.png";

    private OdbcConnection CreateOpenConnection()
    {
        var connection = new OdbcConnection(_connectionString);
        connection.Open();
        return connection;
    }

    private Task<AuthResponse> CreateSessionResponseAsync(OdbcConnection connection, int businessId)
    {
        var token = Guid.NewGuid().ToString("N");
        ExecuteNonQuery(
            connection,
            null,
            """
            INSERT INTO dbo.BusinessSessions (BusinessId, SessionToken, ExpiresAt)
            VALUES (?, ?, DATEADD(DAY, 7, SYSDATETIME()));
            """,
            businessId,
            token);

        return Task.FromResult(new AuthResponse(token, GetBusinessById(connection, null, businessId, publishedOnly: false)));
    }

    private static CreatedOrderResult CreateBusinessOrder(
        OdbcConnection connection,
        OdbcTransaction transaction,
        int businessId,
        string fullName,
        string email,
        string phone,
        string city,
        string address,
        string? notes,
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
                BusinessId,
                CustomerFullName,
                CustomerEmail,
                CustomerPhone,
                CustomerCity,
                DeliveryAddress,
                Notes,
                Status,
                Total
            )
            VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?);
            SELECT CAST(SCOPE_IDENTITY() AS INT);
            """,
            businessId,
            RequireText(fullName, "Ingresa el nombre completo del comprador."),
            NormalizeEmail(email),
            RequireText(phone, "Ingresa un telefono de contacto."),
            RequireText(city, "Ingresa la ciudad de entrega."),
            RequireText(address, "Ingresa la direccion de entrega."),
            NormalizeOptionalText(notes),
            "Pendiente",
            total);

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

    private static List<MarketplaceBusinessDto> GetBusinesses(
        OdbcConnection connection,
        OdbcTransaction? transaction,
        bool publishedOnly,
        int? businessId)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;

        var sql = """
            SELECT
                b.BusinessId,
                b.Slug,
                b.OwnerName,
                b.BusinessName,
                b.Email,
                b.Phone,
                b.City,
                b.Address,
                b.Tagline,
                b.Description,
                b.ShippingLeadTime,
                b.MinimumOrderAmount,
                b.LogoUrl,
                b.BannerUrl,
                b.WebsiteUrl,
                p.ProductId,
                p.Name,
                p.Category,
                p.Description,
                p.Price,
                p.MinimumOrder,
                p.Stock,
                p.ImageUrl,
                p.IsFeatured,
                p.IsPublished
            FROM dbo.Businesses b
            LEFT JOIN dbo.Products p
                ON p.BusinessId = b.BusinessId
                AND p.IsArchived = 0
            """;

        if (publishedOnly)
        {
            sql += " AND p.IsPublished = 1";
        }

        sql += " WHERE 1 = 1";

        if (businessId.HasValue)
        {
            sql += " AND b.BusinessId = ?";
            AddParameter(command, businessId.Value);
        }

        sql += " ORDER BY b.BusinessName, p.IsFeatured DESC, p.Name;";
        command.CommandText = sql;

        using var reader = command.ExecuteReader();
        var businesses = new Dictionary<int, MarketplaceBusinessBuilder>();

        while (reader.Read())
        {
            var currentBusinessId = reader.GetInt32(0);

            if (!businesses.TryGetValue(currentBusinessId, out var builder))
            {
                builder = new MarketplaceBusinessBuilder(
                    reader.GetInt32(0),
                    reader.GetString(1),
                    reader.GetString(2),
                    reader.GetString(3),
                    reader.GetString(4),
                    reader.GetString(5),
                    reader.GetString(6),
                    reader.GetString(7),
                    reader.GetString(8),
                    reader.GetString(9),
                    reader.GetString(10),
                    reader.GetDecimal(11),
                    reader.GetString(12),
                    reader.GetString(13),
                    reader.GetString(14));

                businesses.Add(currentBusinessId, builder);
            }

            if (!reader.IsDBNull(15))
            {
                builder.Products.Add(new ProductDto(
                    reader.GetInt32(15),
                    currentBusinessId,
                    reader.GetString(16),
                    reader.GetString(17),
                    reader.GetString(18),
                    reader.GetDecimal(19),
                    reader.GetInt32(20),
                    reader.GetInt32(21),
                    reader.GetString(22),
                    reader.GetBoolean(23),
                    reader.GetBoolean(24)));
            }
        }

        return businesses.Values
            .Select(builder => builder.Build())
            .ToList();
    }

    private static MarketplaceBusinessDto GetBusinessById(
        OdbcConnection connection,
        OdbcTransaction? transaction,
        int businessId,
        bool publishedOnly)
    {
        var business = GetBusinesses(connection, transaction, publishedOnly, businessId).SingleOrDefault();
        return business ?? throw new InvalidOperationException("La empresa solicitada no existe.");
    }

    private static ProductDto GetOwnedProductById(
        OdbcConnection connection,
        OdbcTransaction? transaction,
        int businessId,
        int productId)
    {
        using var command = CreateCommand(
            connection,
            transaction,
            """
            SELECT ProductId, BusinessId, Name, Category, Description, Price, MinimumOrder, Stock, ImageUrl, IsFeatured, IsPublished
            FROM dbo.Products
            WHERE ProductId = ? AND BusinessId = ? AND IsArchived = 0;
            """,
            productId,
            businessId);

        using var reader = command.ExecuteReader(CommandBehavior.SingleRow);
        if (!reader.Read())
        {
            throw new InvalidOperationException("El producto no existe o no pertenece a tu empresa.");
        }

        return new ProductDto(
            reader.GetInt32(0),
            reader.GetInt32(1),
            reader.GetString(2),
            reader.GetString(3),
            reader.GetString(4),
            reader.GetDecimal(5),
            reader.GetInt32(6),
            reader.GetInt32(7),
            reader.GetString(8),
            reader.GetBoolean(9),
            reader.GetBoolean(10));
    }

    private static int GetBusinessIdByToken(
        OdbcConnection connection,
        OdbcTransaction? transaction,
        string token)
    {
        using var command = CreateCommand(
            connection,
            transaction,
            """
            SELECT BusinessId
            FROM dbo.BusinessSessions
            WHERE SessionToken = ? AND ExpiresAt > SYSDATETIME();
            """,
            token);

        var result = command.ExecuteScalar();
        if (result is null)
        {
            throw new InvalidOperationException("La sesion del empresario no es valida. Inicia sesion nuevamente.");
        }

        return Convert.ToInt32(result);
    }

    private static BusinessOrderPolicy GetBusinessOrderPolicy(
        OdbcConnection connection,
        OdbcTransaction? transaction,
        int businessId)
    {
        using var command = CreateCommand(
            connection,
            transaction,
            "SELECT BusinessName, MinimumOrderAmount FROM dbo.Businesses WHERE BusinessId = ?;",
            businessId);

        using var reader = command.ExecuteReader(CommandBehavior.SingleRow);
        if (!reader.Read())
        {
            throw new InvalidOperationException("La empresa seleccionada ya no esta disponible.");
        }

        return new BusinessOrderPolicy(
            reader.GetString(0),
            reader.GetDecimal(1));
    }

    private static List<BusinessOrderDto> GetBusinessOrdersFeed(
        OdbcConnection connection,
        OdbcTransaction? transaction,
        int businessId)
    {
        var orders = new Dictionary<int, BusinessOrderBuilder>();

        using (var ordersCommand = CreateCommand(
                   connection,
                   transaction,
                   """
                   SELECT
                       OrderId,
                       CustomerFullName,
                       CustomerEmail,
                       CustomerPhone,
                       CustomerCity,
                       DeliveryAddress,
                       Notes,
                       Status,
                       Total,
                       IsNew,
                       CreatedAt
                   FROM dbo.Orders
                   WHERE BusinessId = ?
                   ORDER BY CreatedAt DESC, OrderId DESC;
                   """,
                   businessId))
        {
            using var ordersReader = ordersCommand.ExecuteReader();
            while (ordersReader.Read())
            {
                var orderId = ordersReader.GetInt32(0);
                orders[orderId] = new BusinessOrderBuilder(
                    orderId,
                    ordersReader.GetString(1),
                    ordersReader.GetString(2),
                    ordersReader.GetString(3),
                    ordersReader.GetString(4),
                    ordersReader.GetString(5),
                    ordersReader.IsDBNull(6) ? string.Empty : ordersReader.GetString(6),
                    ordersReader.GetString(7),
                    ordersReader.GetDecimal(8),
                    ordersReader.GetDateTime(10),
                    ordersReader.GetBoolean(9));
            }
        }

        if (orders.Count == 0)
        {
            return [];
        }

        ExecuteNonQuery(
            connection,
            transaction,
            """
            UPDATE dbo.Orders
            SET IsNew = 0,
                ViewedAt = COALESCE(ViewedAt, SYSDATETIME())
            WHERE BusinessId = ? AND IsNew = 1;
            """,
            businessId);

        using (var itemsCommand = CreateCommand(
                   connection,
                   transaction,
                   """
                   SELECT
                       oi.OrderId,
                       oi.ProductId,
                       p.Name,
                       oi.Quantity,
                       oi.UnitPrice,
                       oi.LineTotal
                   FROM dbo.OrderItems oi
                   LEFT JOIN dbo.Products p ON p.ProductId = oi.ProductId
                   INNER JOIN dbo.Orders o ON o.OrderId = oi.OrderId
                   WHERE o.BusinessId = ?
                   ORDER BY oi.OrderId DESC, oi.OrderItemId ASC;
                   """,
                   businessId))
        {
            using var itemsReader = itemsCommand.ExecuteReader();
            while (itemsReader.Read())
            {
                var orderId = itemsReader.GetInt32(0);
                if (!orders.TryGetValue(orderId, out var builder))
                {
                    continue;
                }

                builder.Items.Add(new BusinessOrderItemDto(
                    itemsReader.GetInt32(1),
                    itemsReader.IsDBNull(2) ? "Producto" : itemsReader.GetString(2),
                    itemsReader.GetInt32(3),
                    itemsReader.GetDecimal(4),
                    itemsReader.GetDecimal(5)));
            }
        }

        return orders.Values.Select(order => order.Build()).ToList();
    }

    private static void EnsureProductOwnership(
        OdbcConnection connection,
        OdbcTransaction? transaction,
        int businessId,
        int productId)
    {
        var result = ExecuteScalar(
            connection,
            transaction,
            "SELECT COUNT(*) FROM dbo.Products WHERE ProductId = ? AND BusinessId = ? AND IsArchived = 0;",
            productId,
            businessId);

        if (Convert.ToInt32(result) == 0)
        {
            throw new InvalidOperationException("El producto no existe o no pertenece a tu empresa.");
        }
    }

    private static ProductOrderData GetProductForOrder(
        OdbcConnection connection,
        OdbcTransaction transaction,
        int businessId,
        int productId)
    {
        using var command = CreateCommand(
            connection,
            transaction,
            """
            SELECT ProductId, Name, Price, MinimumOrder, Stock
            FROM dbo.Products
            WHERE ProductId = ? AND BusinessId = ? AND IsPublished = 1 AND IsArchived = 0;
            """,
            productId,
            businessId);

        using var reader = command.ExecuteReader(CommandBehavior.SingleRow);
        if (!reader.Read())
        {
            throw new InvalidOperationException("Uno de los productos del pedido ya no esta disponible.");
        }

        return new ProductOrderData(
            reader.GetInt32(0),
            reader.GetString(1),
            reader.GetDecimal(2),
            reader.GetInt32(3),
            reader.GetInt32(4));
    }

    private static void EnsureEmailAvailable(
        OdbcConnection connection,
        OdbcTransaction? transaction,
        string email,
        int? excludedBusinessId)
    {
        string sql;
        object?[] values;

        if (excludedBusinessId.HasValue)
        {
            sql = "SELECT COUNT(*) FROM dbo.Businesses WHERE Email = ? AND BusinessId <> ?;";
            values = [email, excludedBusinessId.Value];
        }
        else
        {
            sql = "SELECT COUNT(*) FROM dbo.Businesses WHERE Email = ?;";
            values = [email];
        }

        var count = Convert.ToInt32(ExecuteScalar(connection, transaction, sql, values));
        if (count > 0)
        {
            throw new InvalidOperationException("Ya existe una empresa registrada con ese correo.");
        }
    }

    private static string BuildUniqueSlug(
        OdbcConnection connection,
        OdbcTransaction? transaction,
        string businessName,
        int? excludedBusinessId)
    {
        var baseSlug = Slugify(businessName);
        var rootSlug = string.IsNullOrWhiteSpace(baseSlug) ? "empresa" : baseSlug;
        var slug = rootSlug;
        var counter = 2;

        while (SlugExists(connection, transaction, slug, excludedBusinessId))
        {
            slug = $"{rootSlug}-{counter}";
            counter += 1;
        }

        return slug;
    }

    private static bool SlugExists(
        OdbcConnection connection,
        OdbcTransaction? transaction,
        string slug,
        int? excludedBusinessId)
    {
        string sql;
        object?[] values;

        if (excludedBusinessId.HasValue)
        {
            sql = "SELECT COUNT(*) FROM dbo.Businesses WHERE Slug = ? AND BusinessId <> ?;";
            values = [slug, excludedBusinessId.Value];
        }
        else
        {
            sql = "SELECT COUNT(*) FROM dbo.Businesses WHERE Slug = ?;";
            values = [slug];
        }

        return Convert.ToInt32(ExecuteScalar(connection, transaction, sql, values)) > 0;
    }

    private static string NormalizeEmail(string email)
    {
        var normalized = email.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new InvalidOperationException("Ingresa un correo valido.");
        }

        return normalized;
    }

    private static string NormalizeOptionalText(string? value) => value?.Trim() ?? string.Empty;

    private static string RequireText(string value, string errorMessage)
    {
        var normalized = value.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new InvalidOperationException(errorMessage);
        }

        return normalized;
    }

    private static string RequireImageUrl(string value, string fallback)
    {
        var normalized = value.Trim();
        return string.IsNullOrWhiteSpace(normalized) ? fallback : normalized;
    }

    private static decimal RequireMoney(decimal value, string errorMessage = "El precio no puede ser negativo.")
    {
        if (value < 0)
        {
            throw new InvalidOperationException(errorMessage);
        }

        return value;
    }

    private static int RequireMinimumValue(int value, int minimum, string errorMessage)
    {
        if (value < minimum)
        {
            throw new InvalidOperationException(errorMessage);
        }

        return value;
    }

    private static string Slugify(string value) =>
        value
            .Trim()
            .ToLowerInvariant()
            .Normalize(NormalizationForm.FormD)
            .Where(character => CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
            .Aggregate(new StringBuilder(), (builder, character) =>
            {
                if (char.IsLetterOrDigit(character))
                {
                    builder.Append(character);
                }
                else if (builder.Length > 0 && builder[^1] != '-')
                {
                    builder.Append('-');
                }

                return builder;
            })
            .ToString()
            .Trim('-');

    private static void AddParameter(OdbcCommand command, object? value)
    {
        var parameter = command.CreateParameter();
        parameter.Value = value ?? DBNull.Value;
        command.Parameters.Add(parameter);
    }

    private static object? ExecuteScalar(
        OdbcConnection connection,
        OdbcTransaction? transaction,
        string sql,
        params object?[] values)
    {
        using var command = CreateCommand(connection, transaction, sql, values);
        return command.ExecuteScalar();
    }

    private static int InsertAndGetId(
        OdbcConnection connection,
        OdbcTransaction? transaction,
        string sql,
        params object?[] values)
    {
        var result = ExecuteScalar(connection, transaction, sql, values);
        return Convert.ToInt32(result);
    }

    private static int ExecuteNonQuery(
        OdbcConnection connection,
        OdbcTransaction? transaction,
        string sql,
        params object?[] values)
    {
        using var command = CreateCommand(connection, transaction, sql, values);
        return command.ExecuteNonQuery();
    }

    private static OdbcCommand CreateCommand(
        OdbcConnection connection,
        OdbcTransaction? transaction,
        string sql,
        params object?[] values)
    {
        var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = sql;

        foreach (var value in values)
        {
            AddParameter(command, value);
        }

        return command;
    }

    private static string CreateSalt() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(16));

    private static string HashPassword(string password, string base64Salt)
    {
        var salt = Convert.FromBase64String(base64Salt);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            salt,
            100_000,
            HashAlgorithmName.SHA256,
            32);

        return Convert.ToBase64String(hash);
    }

    private static bool VerifyPassword(string password, string base64Salt, string expectedHash) =>
        HashPassword(password, base64Salt) == expectedHash;

    private sealed record ProductOrderData(
        int ProductId,
        string Name,
        decimal Price,
        int MinimumOrder,
        int Stock);

    private sealed record CreatedOrderResult(
        int OrderId,
        int BusinessId,
        string BusinessName,
        decimal Total);

    private sealed record BusinessOrderPolicy(
        string BusinessName,
        decimal MinimumOrderAmount);

    private sealed class BusinessOrderBuilder(
        int orderId,
        string customerFullName,
        string customerEmail,
        string customerPhone,
        string customerCity,
        string deliveryAddress,
        string notes,
        string status,
        decimal total,
        DateTime createdAt,
        bool isNew)
    {
        public List<BusinessOrderItemDto> Items { get; } = [];

        public BusinessOrderDto Build() => new(
            orderId,
            customerFullName,
            customerEmail,
            customerPhone,
            customerCity,
            deliveryAddress,
            notes,
            status,
            total,
            createdAt,
            isNew,
            Items);
    }

    private sealed class MarketplaceBusinessBuilder(
        int businessId,
        string slug,
        string ownerName,
        string businessName,
        string email,
        string phone,
        string city,
        string address,
        string tagline,
        string description,
        string shippingLeadTime,
        decimal minimumOrderAmount,
        string logoUrl,
        string bannerUrl,
        string websiteUrl)
    {
        public List<ProductDto> Products { get; } = [];

        public MarketplaceBusinessDto Build() => new(
            businessId,
            slug,
            ownerName,
            businessName,
            email,
            phone,
            city,
            address,
            tagline,
            description,
            shippingLeadTime,
            minimumOrderAmount,
            logoUrl,
            bannerUrl,
            websiteUrl,
            Products);
    }
}
