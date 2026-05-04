using System.Globalization;
using System.Text;
using TiendaMicroempresas.Api.Contracts;

namespace TiendaMicroempresas.Api.Repositories;

internal static class DemoStoreRuntime
{
    private const string DefaultPassword = "Empresa2026!";
    private const string DefaultLogoUrl = "/assets/images/store1.png";
    private const string DefaultBannerUrl = "/assets/images/banner-localshop-default.jpg";
    private const string DefaultProductImageUrl = "/assets/images/pla1.png";
    private static readonly Lock Sync = new();
    private static readonly List<BusinessRecord> Businesses = CreateSeedBusinesses();
    private static readonly List<OrderRecord> Orders = [];
    private static readonly Dictionary<string, int> Sessions = [];
    private static int _nextBusinessId = Businesses.Max(business => business.BusinessId) + 1;
    private static int _nextProductId = Businesses.SelectMany(business => business.Products).Max(product => product.ProductId) + 1;
    private static int _nextOrderId = 1000;

    public static bool HasBusinesses()
    {
        lock (Sync)
        {
            return Businesses.Count > 0;
        }
    }

    public static StoreOverviewResponse GetOverview()
    {
        lock (Sync)
        {
            return BuildOverview();
        }
    }

    public static AuthResponse RegisterOwner(RegisterOwnerRequest request)
    {
        lock (Sync)
        {
            var email = NormalizeEmail(request.Email);
            if (Businesses.Any(business => business.Email.Equals(email, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException("Ya existe una empresa registrada con ese correo.");
            }

            var businessName = RequireText(request.BusinessName, "Ingresa el nombre de la empresa.");
            var business = new BusinessRecord
            {
                BusinessId = _nextBusinessId++,
                Slug = BuildUniqueSlug(businessName, null),
                OwnerName = RequireText(request.OwnerName, "Ingresa el nombre de la persona responsable."),
                BusinessName = businessName,
                Email = email,
                Password = RequireText(request.Password, "Ingresa una contrasena."),
                Phone = RequireText(request.Phone, "Ingresa un telefono de contacto."),
                City = RequireText(request.City, "Ingresa la ciudad principal de operacion."),
                Address = RequireText(request.Address, "Ingresa la direccion comercial."),
                Tagline = RequireText(request.Tagline, "Ingresa una frase corta para presentar la empresa."),
                Description = RequireText(request.Description, "Ingresa una descripcion clara del negocio."),
                ShippingLeadTime = RequireText(request.ShippingLeadTime, "Ingresa el tiempo estimado de entrega."),
                MinimumOrderAmount = RequireMoney(request.MinimumOrderAmount, "El pedido minimo por empresa no puede ser negativo."),
                LogoUrl = DefaultLogoUrl,
                BannerUrl = DefaultBannerUrl,
                WebsiteUrl = request.WebsiteUrl?.Trim() ?? string.Empty,
            };

            Businesses.Add(business);
            return CreateSession(business.BusinessId);
        }
    }

    public static AuthResponse LoginOwner(LoginOwnerRequest request)
    {
        lock (Sync)
        {
            var email = NormalizeEmail(request.Email);
            var business = Businesses.SingleOrDefault(item => item.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
            if (business is null || business.Password != request.Password)
            {
                throw new InvalidOperationException("Correo o contrasena incorrectos.");
            }

            return CreateSession(business.BusinessId);
        }
    }

    public static MarketplaceBusinessDto GetBusinessByToken(string token)
    {
        lock (Sync)
        {
            return ToDto(GetBusinessRecordByToken(token));
        }
    }

    public static MarketplaceBusinessDto UpdateOwnerProfile(string token, UpdateOwnerProfileRequest request)
    {
        lock (Sync)
        {
            var business = GetBusinessRecordByToken(token);
            var email = NormalizeEmail(request.Email);
            if (Businesses.Any(item => item.BusinessId != business.BusinessId && item.Email.Equals(email, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException("Ya existe una empresa registrada con ese correo.");
            }

            business.OwnerName = RequireText(request.OwnerName, "Ingresa el nombre de la persona responsable.");
            business.BusinessName = RequireText(request.BusinessName, "Ingresa el nombre de la empresa.");
            business.Email = email;
            business.Phone = RequireText(request.Phone, "Ingresa un telefono de contacto.");
            business.City = RequireText(request.City, "Ingresa la ciudad principal de operacion.");
            business.Address = RequireText(request.Address, "Ingresa la direccion comercial.");
            business.Tagline = RequireText(request.Tagline, "Ingresa una frase corta para presentar la empresa.");
            business.Description = RequireText(request.Description, "Ingresa una descripcion clara del negocio.");
            business.ShippingLeadTime = RequireText(request.ShippingLeadTime, "Ingresa el tiempo estimado de entrega.");
            business.MinimumOrderAmount = RequireMoney(request.MinimumOrderAmount, "El pedido minimo por empresa no puede ser negativo.");
            business.LogoUrl = string.IsNullOrWhiteSpace(request.LogoUrl) ? DefaultLogoUrl : request.LogoUrl.Trim();
            business.BannerUrl = string.IsNullOrWhiteSpace(request.BannerUrl) ? DefaultBannerUrl : request.BannerUrl.Trim();
            business.WebsiteUrl = request.WebsiteUrl?.Trim() ?? string.Empty;
            business.Slug = BuildUniqueSlug(business.BusinessName, business.BusinessId);

            return ToDto(business);
        }
    }

    public static void Logout(string token)
    {
        lock (Sync)
        {
            Sessions.Remove(token);
        }
    }

    public static AdminCatalogResponse GetAdminCatalog(string token)
    {
        lock (Sync)
        {
            var business = GetBusinessRecordByToken(token);
            var categories = business.Products
                .Select(product => product.Category)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(category => category, StringComparer.OrdinalIgnoreCase)
                .ToList();

            return new AdminCatalogResponse(ToDto(business), categories);
        }
    }

    public static ProductDto CreateProduct(string token, UpsertProductRequest request)
    {
        lock (Sync)
        {
            var business = GetBusinessRecordByToken(token);
            var product = new ProductRecord
            {
                ProductId = _nextProductId++,
                BusinessId = business.BusinessId,
                Name = RequireText(request.Name, "Ingresa un nombre para el producto."),
                Category = RequireText(request.Category, "Ingresa una categoria para el producto."),
                Description = RequireText(request.Description, "Ingresa una descripcion para el producto."),
                Price = RequireMoney(request.Price),
                MinimumOrder = RequireMinimumValue(request.MinimumOrder, 1, "El pedido minimo debe ser al menos 1."),
                Stock = RequireMinimumValue(request.Stock, 0, "El stock no puede ser negativo."),
                ImageUrl = string.IsNullOrWhiteSpace(request.ImageUrl) ? DefaultProductImageUrl : request.ImageUrl.Trim(),
                IsFeatured = request.IsFeatured,
                IsPublished = request.IsPublished
            };

            business.Products.Add(product);
            return ToProductDto(product);
        }
    }

    public static ProductDto UpdateProduct(string token, int productId, UpsertProductRequest request)
    {
        lock (Sync)
        {
            var business = GetBusinessRecordByToken(token);
            var product = business.Products.SingleOrDefault(item => item.ProductId == productId)
                ?? throw new InvalidOperationException("El producto no existe o no pertenece a tu empresa.");

            product.Name = RequireText(request.Name, "Ingresa un nombre para el producto.");
            product.Category = RequireText(request.Category, "Ingresa una categoria para el producto.");
            product.Description = RequireText(request.Description, "Ingresa una descripcion para el producto.");
            product.Price = RequireMoney(request.Price);
            product.MinimumOrder = RequireMinimumValue(request.MinimumOrder, 1, "El pedido minimo debe ser al menos 1.");
            product.Stock = RequireMinimumValue(request.Stock, 0, "El stock no puede ser negativo.");
            product.ImageUrl = string.IsNullOrWhiteSpace(request.ImageUrl) ? DefaultProductImageUrl : request.ImageUrl.Trim();
            product.IsFeatured = request.IsFeatured;
            product.IsPublished = request.IsPublished;

            return ToProductDto(product);
        }
    }

    public static void DeleteProduct(string token, int productId)
    {
        lock (Sync)
        {
            var business = GetBusinessRecordByToken(token);
            var removed = business.Products.RemoveAll(item => item.ProductId == productId);
            if (removed == 0)
            {
                throw new InvalidOperationException("El producto no existe o no pertenece a tu empresa.");
            }
        }
    }

    public static OrderCreatedResponse CreateOrder(CreateOrderRequest request)
    {
        lock (Sync)
        {
            var createdOrder = CreateOrderInternal(
                request.BusinessId,
                request.FullName,
                request.Email,
                request.Phone,
                request.City,
                request.Address,
                request.Notes,
                request.Items.Select(item => (item.ProductId, item.Quantity)).ToList());

            return new OrderCreatedResponse(
                createdOrder.OrderId,
                createdOrder.Total,
                $"Pedido enviado a {createdOrder.BusinessName}. El equipo comercial revisara tu solicitud.");
        }
    }

    public static CheckoutCartResponse CheckoutCart(CheckoutCartRequest request)
    {
        lock (Sync)
        {
            if (request.Items.Count == 0)
            {
                throw new InvalidOperationException("Debes agregar al menos un producto antes de comprar.");
            }

            var orderRequests = request.Items
                .GroupBy(item => item.BusinessId)
                .Select(group => new
                {
                    BusinessId = group.Key,
                    Items = group.Select(item => (item.ProductId, item.Quantity)).ToList()
                })
                .ToList();

            var createdOrders = new List<CreatedOrderResult>();
            foreach (var orderRequest in orderRequests)
            {
                createdOrders.Add(CreateOrderInternal(
                    orderRequest.BusinessId,
                    request.FullName,
                    request.Email,
                    request.Phone,
                    request.City,
                    request.Address,
                    request.Notes,
                    orderRequest.Items));
            }

            var total = createdOrders.Sum(order => order.Total);
            return new CheckoutCartResponse(
                createdOrders.Count,
                total,
                $"Se registraron {createdOrders.Count} pedidos para tus empresas seleccionadas.",
                createdOrders.Select(order => new BusinessCheckoutResultDto(order.OrderId, order.BusinessId, order.BusinessName, order.Total)).ToList());
        }
    }

    public static BusinessOrdersFeedResponse GetBusinessOrders(string token)
    {
        lock (Sync)
        {
            var business = GetBusinessRecordByToken(token);
            var businessOrders = Orders
                .Where(order => order.BusinessId == business.BusinessId)
                .OrderByDescending(order => order.CreatedAt)
                .ToList();

            var newOrders = businessOrders.Count(order => order.IsNew);
            var response = new BusinessOrdersFeedResponse(
                newOrders,
                businessOrders.Select(order => new BusinessOrderDto(
                    order.OrderId,
                    order.CustomerFullName,
                    order.CustomerEmail,
                    order.CustomerPhone,
                    order.CustomerCity,
                    order.DeliveryAddress,
                    order.Notes,
                    order.Status,
                    order.Total,
                    order.CreatedAt,
                    order.IsNew,
                    order.Items.Select(item => new BusinessOrderItemDto(
                        item.ProductId,
                        item.ProductName,
                        item.Quantity,
                        item.UnitPrice,
                        item.LineTotal)).ToList())).ToList());

            foreach (var order in businessOrders.Where(order => order.IsNew))
            {
                order.IsNew = false;
                order.ViewedAt = DateTime.UtcNow;
            }

            return response;
        }
    }

    private static AuthResponse CreateSession(int businessId)
    {
        var token = Guid.NewGuid().ToString("N");
        Sessions[token] = businessId;
        var business = Businesses.Single(item => item.BusinessId == businessId);
        return new AuthResponse(token, ToDto(business));
    }

    private static MarketplaceBusinessDto ToDto(BusinessRecord business) =>
        new(
            business.BusinessId,
            business.Slug,
            business.OwnerName,
            business.BusinessName,
            business.Email,
            business.Phone,
            business.City,
            business.Address,
            business.Tagline,
            business.Description,
            business.ShippingLeadTime,
            business.MinimumOrderAmount,
            business.LogoUrl,
            business.BannerUrl,
            business.WebsiteUrl,
            business.Products.Select(ToProductDto).ToList());

    private static ProductDto ToProductDto(ProductRecord product) =>
        new(
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
            product.IsPublished);

    private static StoreOverviewResponse BuildOverview()
    {
        var businesses = Businesses.Select(ToDto).ToList();
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
            .ToList();

        var categories = businesses
            .SelectMany(business => business.Products)
            .Select(product => product.Category)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(category => category, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return new StoreOverviewResponse(
            businesses,
            featuredProducts,
            categories,
            businesses.Count,
            businesses.Sum(business => business.Products.Count));
    }

    private static BusinessRecord GetBusinessRecordByToken(string token)
    {
        if (!Sessions.TryGetValue(token, out var businessId))
        {
            throw new InvalidOperationException("La sesion del empresario no es valida. Inicia sesion nuevamente.");
        }

        return Businesses.Single(item => item.BusinessId == businessId);
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

    private static string RequireText(string value, string errorMessage)
    {
        var normalized = value.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new InvalidOperationException(errorMessage);
        }

        return normalized;
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

    private static string BuildUniqueSlug(string businessName, int? excludedBusinessId)
    {
        var root = Slugify(businessName);
        var slug = string.IsNullOrWhiteSpace(root) ? "empresa" : root;
        var candidate = slug;
        var counter = 2;

        while (Businesses.Any(item =>
                   item.Slug.Equals(candidate, StringComparison.OrdinalIgnoreCase)
                   && (!excludedBusinessId.HasValue || item.BusinessId != excludedBusinessId.Value)))
        {
            candidate = $"{slug}-{counter}";
            counter++;
        }

        return candidate;
    }

    private static string Slugify(string value)
    {
        var normalized = value.ToLowerInvariant().Normalize(NormalizationForm.FormD);
        Span<char> buffer = stackalloc char[normalized.Length];
        var index = 0;
        var dash = false;

        foreach (var character in normalized)
        {
            if (char.GetUnicodeCategory(character) == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            if (char.IsLetterOrDigit(character))
            {
                buffer[index++] = character;
                dash = false;
            }
            else if (!dash && index > 0)
            {
                buffer[index++] = '-';
                dash = true;
            }
        }

        return new string(buffer[..index]).Trim('-');
    }

    private static CreatedOrderResult CreateOrderInternal(
        int businessId,
        string fullName,
        string email,
        string phone,
        string city,
        string address,
        string? notes,
        IReadOnlyList<(int ProductId, int Quantity)> items)
    {
        var business = Businesses.SingleOrDefault(item => item.BusinessId == businessId)
            ?? throw new InvalidOperationException("La empresa seleccionada ya no esta disponible.");

        var total = 0m;
        var orderItems = new List<OrderItemRecord>();

        foreach (var item in items)
        {
            var product = business.Products.SingleOrDefault(product => product.ProductId == item.ProductId && product.IsPublished)
                ?? throw new InvalidOperationException("Uno de los productos del pedido ya no esta disponible.");

            if (item.Quantity <= 0)
            {
                throw new InvalidOperationException("Todas las cantidades del pedido deben ser mayores a cero.");
            }

            if (item.Quantity > product.Stock)
            {
                throw new InvalidOperationException($"No hay inventario suficiente para {product.Name}. Stock disponible: {product.Stock}.");
            }

            var lineTotal = item.Quantity * product.Price;
            total += lineTotal;
            orderItems.Add(new OrderItemRecord
            {
                ProductId = product.ProductId,
                ProductName = product.Name,
                Quantity = item.Quantity,
                UnitPrice = product.Price,
                LineTotal = lineTotal,
            });
        }

        if (total < business.MinimumOrderAmount)
        {
            throw new InvalidOperationException($"El pedido minimo para {business.BusinessName} es de {business.MinimumOrderAmount.ToString("C0", CultureInfo.GetCultureInfo("es-CO"))}.");
        }

        foreach (var item in items)
        {
            var product = business.Products.Single(product => product.ProductId == item.ProductId);
            product.Stock -= item.Quantity;
        }

        var order = new OrderRecord
        {
            OrderId = _nextOrderId++,
            BusinessId = business.BusinessId,
            BusinessName = business.BusinessName,
            CustomerFullName = RequireText(fullName, "Ingresa el nombre completo del comprador."),
            CustomerEmail = NormalizeEmail(email),
            CustomerPhone = RequireText(phone, "Ingresa un telefono de contacto."),
            CustomerCity = RequireText(city, "Ingresa la ciudad de entrega."),
            DeliveryAddress = RequireText(address, "Ingresa la direccion de entrega."),
            Notes = notes?.Trim() ?? string.Empty,
            Status = "Pendiente",
            IsNew = true,
            CreatedAt = DateTime.UtcNow,
            Total = total,
            Items = orderItems,
        };

        Orders.Add(order);
        return new CreatedOrderResult(order.OrderId, business.BusinessId, business.BusinessName, total);
    }

    private static List<BusinessRecord> CreateSeedBusinesses() =>
    [
        new BusinessRecord
        {
            BusinessId = 1,
            Slug = "andes-pack-studio",
            OwnerName = "Mariana Cardenas",
            BusinessName = "Andes Pack Studio",
            Email = "contacto@andespack.co",
            Password = DefaultPassword,
            Phone = "+57 604 322 4410",
            City = "Medellin",
            Address = "Cra. 43A #18 Sur-135, El Poblado, Medellin",
            Tagline = "Packaging corporativo reutilizable para marcas que quieren vender mejor.",
            Description = "Empresa especializada en empaques premium, textiles reutilizables y presentaciones corporativas para marcas que necesitan elevar su experiencia de entrega sin perder eficiencia operativa.",
            ShippingLeadTime = "Entregas en Medellin en 24 horas y despachos nacionales entre 2 y 4 dias habiles.",
            MinimumOrderAmount = 30000,
            LogoUrl = "/assets/images/store2.png",
            BannerUrl = "/assets/images/banner-andes-pack-studio.jpg",
            WebsiteUrl = "https://andespack.co",
            Products =
            [
                new ProductRecord { ProductId = 1, BusinessId = 1, Name = "Impresora termica para etiquetado logistico", Category = "Empaques corporativos", Description = "Equipo compacto para imprimir etiquetas de despacho, referencias internas y control de inventario en operaciones comerciales.", Price = 389000, MinimumOrder = 1, Stock = 24, ImageUrl = "/assets/images/pla1.png", IsFeatured = true, IsPublished = true },
                new ProductRecord { ProductId = 2, BusinessId = 1, Name = "Rollo premium de etiquetas adhesivas", Category = "Empaques sostenibles", Description = "Consumible para procesos de alistamiento, trazabilidad de pedidos y presentacion profesional del empaque.", Price = 42000, MinimumOrder = 1, Stock = 220, ImageUrl = "/assets/images/pla2.png", IsFeatured = false, IsPublished = true },
                new ProductRecord { ProductId = 3, BusinessId = 1, Name = "Lector de codigo de barras para bodega", Category = "Presentacion de marca", Description = "Solucion de lectura rapida para control de inventario, despacho y validacion de referencias en puntos de empaque.", Price = 165000, MinimumOrder = 1, Stock = 90, ImageUrl = "/assets/images/pla3.png", IsFeatured = true, IsPublished = true }
            ]
        },
        new BusinessRecord
        {
            BusinessId = 2,
            Slug = "aura-cafe-ejecutivo",
            OwnerName = "Valentina Ruiz",
            BusinessName = "Aura Cafe Ejecutivo",
            Email = "direccion@auracafe.co",
            Password = DefaultPassword,
            Phone = "+57 601 745 8890",
            City = "Bogota",
            Address = "Calle 85 #12-36, Chapinero, Bogota",
            Tagline = "Coffee breaks, desayunos y hospitalidad ejecutiva para reuniones, eventos y equipos comerciales.",
            Description = "Firma enfocada en experiencias de hospitalidad para oficinas y eventos empresariales, con formatos de coffee break, brunch corporativo y atencion alimentaria para jornadas comerciales o institucionales.",
            ShippingLeadTime = "Cobertura en Bogota el mismo dia y envios nacionales entre 24 y 72 horas.",
            MinimumOrderAmount = 45000,
            LogoUrl = "/assets/images/store1.png",
            BannerUrl = "/assets/images/banner-aura-cafe-ejecutivo.jpg",
            WebsiteUrl = "https://auracafe.co",
            Products =
            [
                new ProductRecord { ProductId = 4, BusinessId = 2, Name = "Coffee break ejecutivo para reuniones", Category = "Hospitalidad empresarial", Description = "Montaje de desayuno corporativo con pasteleria ligera, fruta y bebidas para juntas de direccion o visitas comerciales.", Price = 69000, MinimumOrder = 1, Stock = 60, ImageUrl = "/assets/images/breakfast.png", IsFeatured = true, IsPublished = true },
                new ProductRecord { ProductId = 5, BusinessId = 2, Name = "Brunch artesanal para onboarding", Category = "Desayunos corporativos", Description = "Formato de brunch con pancakes y complementos pensado para sesiones de bienvenida, workshops internos y activaciones de marca.", Price = 36000, MinimumOrder = 1, Stock = 120, ImageUrl = "/assets/images/b2.png", IsFeatured = false, IsPublished = true },
                new ProductRecord { ProductId = 6, BusinessId = 2, Name = "Almuerzo premium para eventos internos", Category = "Eventos empresariales", Description = "Opcion de alimentacion ejecutiva para jornadas extendidas, comites internos y encuentros con clientes.", Price = 54000, MinimumOrder = 1, Stock = 75, ImageUrl = "/assets/images/b1.png", IsFeatured = true, IsPublished = true }
            ]
        },
        new BusinessRecord
        {
            BusinessId = 3,
            Slug = "lumen-verde-bienestar",
            OwnerName = "Sergio Latorre",
            BusinessName = "Lumen Verde Bienestar",
            Email = "comercial@lumenverde.co",
            Password = DefaultPassword,
            Phone = "+57 602 485 6612",
            City = "Cali",
            Address = "Avenida 6N #28N-45, Granada, Cali",
            Tagline = "Bienestar corporativo con estaciones saludables, bowls frescos y soluciones de alimentacion ligera.",
            Description = "Equipo especializado en experiencias de bienestar para empresas, con propuestas de fruta fresca, bowls funcionales y menus ligeros para jornadas internas, recepciones y espacios colaborativos.",
            ShippingLeadTime = "Produccion entre 24 y 48 horas segun inventario y cobertura nacional desde Cali.",
            MinimumOrderAmount = 35000,
            LogoUrl = "/assets/images/store1.png",
            BannerUrl = "/assets/images/banner-lumen-verde-bienestar.jpg",
            WebsiteUrl = "https://lumenverde.co",
            Products =
            [
                new ProductRecord { ProductId = 7, BusinessId = 3, Name = "Estacion de frutas frescas para oficina", Category = "Bienestar corporativo", Description = "Servicio de fruta fresca presentada para recepciones, oficinas de alta rotacion y programas internos de bienestar.", Price = 28000, MinimumOrder = 1, Stock = 80, ImageUrl = "/assets/images/b4.png", IsFeatured = true, IsPublished = true },
                new ProductRecord { ProductId = 8, BusinessId = 3, Name = "Bowl funcional para jornadas de trabajo", Category = "Alimentacion saludable", Description = "Bowl ligero con ingredientes frescos pensado para reuniones, sesiones creativas y equipos operativos.", Price = 43000, MinimumOrder = 1, Stock = 65, ImageUrl = "/assets/images/pl-12.png", IsFeatured = true, IsPublished = true },
                new ProductRecord { ProductId = 9, BusinessId = 3, Name = "Menu saludable para comite ejecutivo", Category = "Wellness corporativo", Description = "Preparacion balanceada para atencion empresarial, pausas activas y espacios de bienestar organizacional.", Price = 48000, MinimumOrder = 1, Stock = 55, ImageUrl = "/assets/images/pl-1.png", IsFeatured = false, IsPublished = true }
            ]
        }
    ];

    private sealed class BusinessRecord
    {
        public int BusinessId { get; set; }
        public string Slug { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public string BusinessName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Tagline { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ShippingLeadTime { get; set; } = string.Empty;
        public decimal MinimumOrderAmount { get; set; }
        public string LogoUrl { get; set; } = string.Empty;
        public string BannerUrl { get; set; } = string.Empty;
        public string WebsiteUrl { get; set; } = string.Empty;
        public List<ProductRecord> Products { get; set; } = [];
    }

    private sealed class ProductRecord
    {
        public int ProductId { get; set; }
        public int BusinessId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int MinimumOrder { get; set; }
        public int Stock { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsFeatured { get; set; }
        public bool IsPublished { get; set; }
    }

    private sealed class OrderRecord
    {
        public int OrderId { get; set; }
        public int BusinessId { get; set; }
        public string BusinessName { get; set; } = string.Empty;
        public string CustomerFullName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public string CustomerCity { get; set; } = string.Empty;
        public string DeliveryAddress { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public bool IsNew { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ViewedAt { get; set; }
        public decimal Total { get; set; }
        public List<OrderItemRecord> Items { get; set; } = [];
    }

    private sealed class OrderItemRecord
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
    }

    private sealed record CreatedOrderResult(
        int OrderId,
        int BusinessId,
        string BusinessName,
        decimal Total);
}
