/*
    Inicializacion idempotente para Docker.

    A diferencia de init.sql, este archivo no borra datos. Crea la base,
    crea tablas faltantes, aplica columnas nuevas y si la base esta vacia
    carga las 3 empresas demo.
*/

IF DB_ID(N'TiendaMicroempresas') IS NULL
BEGIN
    CREATE DATABASE TiendaMicroempresas;
END;
GO

USE TiendaMicroempresas;
GO

IF OBJECT_ID(N'dbo.Businesses', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Businesses
    (
        BusinessId INT IDENTITY(1,1) PRIMARY KEY,
        Slug NVARCHAR(120) NOT NULL UNIQUE,
        OwnerName NVARCHAR(120) NOT NULL,
        BusinessName NVARCHAR(160) NOT NULL,
        Email NVARCHAR(160) NOT NULL UNIQUE,
        PasswordHash NVARCHAR(255) NOT NULL,
        PasswordSalt NVARCHAR(255) NOT NULL,
        Phone NVARCHAR(40) NOT NULL,
        City NVARCHAR(80) NOT NULL,
        Address NVARCHAR(220) NOT NULL,
        Tagline NVARCHAR(180) NOT NULL,
        Description NVARCHAR(900) NOT NULL,
        ShippingLeadTime NVARCHAR(180) NOT NULL,
        MinimumOrderAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
        LogoUrl NVARCHAR(255) NOT NULL,
        BannerUrl NVARCHAR(255) NOT NULL,
        WebsiteUrl NVARCHAR(255) NOT NULL DEFAULT N'',
        CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME()
    );
END;
GO

IF OBJECT_ID(N'dbo.BusinessSessions', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.BusinessSessions
    (
        BusinessSessionId INT IDENTITY(1,1) PRIMARY KEY,
        BusinessId INT NOT NULL,
        SessionToken NVARCHAR(120) NOT NULL UNIQUE,
        ExpiresAt DATETIME2 NOT NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
        CONSTRAINT FK_BusinessSessions_Businesses FOREIGN KEY (BusinessId) REFERENCES dbo.Businesses(BusinessId)
    );
END;
GO

IF OBJECT_ID(N'dbo.Products', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Products
    (
        ProductId INT IDENTITY(1,1) PRIMARY KEY,
        BusinessId INT NOT NULL,
        Name NVARCHAR(160) NOT NULL,
        Category NVARCHAR(100) NOT NULL,
        Description NVARCHAR(700) NOT NULL,
        Price DECIMAL(18,2) NOT NULL,
        MinimumOrder INT NOT NULL,
        Stock INT NOT NULL,
        ImageUrl NVARCHAR(255) NOT NULL,
        IsFeatured BIT NOT NULL DEFAULT 0,
        IsPublished BIT NOT NULL DEFAULT 1,
        IsArchived BIT NOT NULL DEFAULT 0,
        CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
        CONSTRAINT FK_Products_Businesses FOREIGN KEY (BusinessId) REFERENCES dbo.Businesses(BusinessId)
    );
END;
GO

IF COL_LENGTH(N'dbo.Products', N'IsArchived') IS NULL
BEGIN
    ALTER TABLE dbo.Products
    ADD IsArchived BIT NOT NULL
        CONSTRAINT DF_Products_IsArchived DEFAULT 0;
END;
GO

IF OBJECT_ID(N'dbo.Orders', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Orders
    (
        OrderId INT IDENTITY(1,1) PRIMARY KEY,
        BusinessId INT NOT NULL,
        CustomerFullName NVARCHAR(160) NOT NULL,
        CustomerEmail NVARCHAR(160) NOT NULL,
        CustomerPhone NVARCHAR(40) NOT NULL,
        CustomerCity NVARCHAR(80) NOT NULL,
        DeliveryAddress NVARCHAR(220) NOT NULL,
        Notes NVARCHAR(500) NULL,
        Status NVARCHAR(40) NOT NULL,
        IsNew BIT NOT NULL DEFAULT 1,
        ViewedAt DATETIME2 NULL,
        Total DECIMAL(18,2) NOT NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
        CONSTRAINT FK_Orders_Businesses FOREIGN KEY (BusinessId) REFERENCES dbo.Businesses(BusinessId)
    );
END;
GO

IF OBJECT_ID(N'dbo.OrderItems', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.OrderItems
    (
        OrderItemId INT IDENTITY(1,1) PRIMARY KEY,
        OrderId INT NOT NULL,
        ProductId INT NOT NULL,
        Quantity INT NOT NULL,
        UnitPrice DECIMAL(18,2) NOT NULL,
        LineTotal DECIMAL(18,2) NOT NULL,
        CONSTRAINT FK_OrderItems_Orders FOREIGN KEY (OrderId) REFERENCES dbo.Orders(OrderId),
        CONSTRAINT FK_OrderItems_Products FOREIGN KEY (ProductId) REFERENCES dbo.Products(ProductId)
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Products_BusinessId' AND object_id = OBJECT_ID(N'dbo.Products'))
BEGIN
    CREATE INDEX IX_Products_BusinessId ON dbo.Products (BusinessId);
END;
GO

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Products_PublishedFeatured' AND object_id = OBJECT_ID(N'dbo.Products'))
BEGIN
    DROP INDEX IX_Products_PublishedFeatured ON dbo.Products;
END;
GO

CREATE INDEX IX_Products_PublishedFeatured ON dbo.Products (IsArchived, IsPublished, IsFeatured);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Orders_BusinessId' AND object_id = OBJECT_ID(N'dbo.Orders'))
BEGIN
    CREATE INDEX IX_Orders_BusinessId ON dbo.Orders (BusinessId);
END;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Businesses)
BEGIN
    SET IDENTITY_INSERT dbo.Businesses ON;

    INSERT INTO dbo.Businesses
    (
        BusinessId, Slug, OwnerName, BusinessName, Email, PasswordHash, PasswordSalt,
        Phone, City, Address, Tagline, Description, ShippingLeadTime,
        MinimumOrderAmount, LogoUrl, BannerUrl, WebsiteUrl
    )
    VALUES
    (1, N'andes-pack-studio', N'Mariana Cardenas', N'Andes Pack Studio', N'contacto@andespack.co', N'eulOwHJN9Z8F4/GGk74KA2QM/ddudtUGEGyUlsDrrfM=', N'b/GV47uErm89GXuh1mbVaw==', N'+57 604 322 4410', N'Medellin', N'Cra. 43A #18 Sur-135, El Poblado, Medellin', N'Packaging corporativo reutilizable para marcas que quieren vender mejor.', N'Empresa especializada en empaques premium, textiles reutilizables y presentaciones corporativas para marcas que necesitan elevar su experiencia de entrega sin perder eficiencia operativa.', N'Entregas en Medellin en 24 horas y despachos nacionales entre 2 y 4 dias habiles.', 30000, N'/assets/images/store2.png', N'/assets/images/banner-andes-pack-studio.jpg', N'https://andespack.co'),
    (2, N'aura-cafe-ejecutivo', N'Valentina Ruiz', N'Aura Cafe Ejecutivo', N'direccion@auracafe.co', N'oOGs3jemViisBU5N5jLxvi9wtw5E4gXSON91orrQfcc=', N'r+TMMNXdUFQc5ldz9YQ0Og==', N'+57 601 745 8890', N'Bogota', N'Calle 85 #12-36, Chapinero, Bogota', N'Coffee breaks, desayunos y hospitalidad ejecutiva para reuniones, eventos y equipos comerciales.', N'Firma enfocada en experiencias de hospitalidad para oficinas y eventos empresariales, con formatos de coffee break, brunch corporativo y atencion alimentaria para jornadas comerciales o institucionales.', N'Cobertura en Bogota el mismo dia y envios nacionales entre 24 y 72 horas.', 45000, N'/assets/images/store1.png', N'/assets/images/banner-aura-cafe-ejecutivo.jpg', N'https://auracafe.co'),
    (3, N'lumen-verde-bienestar', N'Sergio Latorre', N'Lumen Verde Bienestar', N'comercial@lumenverde.co', N'Q9dCftfYnDkfmhmFSPClZht4QtElQ+o207SS/LF3EnI=', N'hrMv67mhwOkZr2FA9edzfA==', N'+57 602 485 6612', N'Cali', N'Avenida 6N #28N-45, Granada, Cali', N'Bienestar corporativo con estaciones saludables, bowls frescos y soluciones de alimentacion ligera.', N'Equipo especializado en experiencias de bienestar para empresas, con propuestas de fruta fresca, bowls funcionales y menus ligeros para jornadas internas, recepciones y espacios colaborativos.', N'Produccion entre 24 y 48 horas segun inventario y cobertura nacional desde Cali.', 35000, N'/assets/images/store1.png', N'/assets/images/banner-lumen-verde-bienestar.jpg', N'https://lumenverde.co');

    SET IDENTITY_INSERT dbo.Businesses OFF;
END;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Products)
BEGIN
    SET IDENTITY_INSERT dbo.Products ON;

    INSERT INTO dbo.Products
    (
        ProductId, BusinessId, Name, Category, Description, Price, MinimumOrder,
        Stock, ImageUrl, IsFeatured, IsPublished, IsArchived
    )
    VALUES
        (1, 1, N'Impresora termica para etiquetado logistico', N'Empaques corporativos', N'Equipo compacto para imprimir etiquetas de despacho, referencias internas y control de inventario en operaciones comerciales.', 389000, 1, 24, N'/assets/images/pla1.png', 1, 1, 0),
        (2, 1, N'Rollo premium de etiquetas adhesivas', N'Empaques sostenibles', N'Consumible para procesos de alistamiento, trazabilidad de pedidos y presentacion profesional del empaque.', 42000, 1, 220, N'/assets/images/pla2.png', 0, 1, 0),
        (3, 1, N'Lector de codigo de barras para bodega', N'Presentacion de marca', N'Solucion de lectura rapida para control de inventario, despacho y validacion de referencias en puntos de empaque.', 165000, 1, 90, N'/assets/images/pla3.png', 1, 1, 0),
        (4, 2, N'Coffee break ejecutivo para reuniones', N'Hospitalidad empresarial', N'Montaje de desayuno corporativo con pasteleria ligera, fruta y bebidas para juntas de direccion o visitas comerciales.', 69000, 1, 60, N'/assets/images/breakfast.png', 1, 1, 0),
        (5, 2, N'Brunch artesanal para onboarding', N'Desayunos corporativos', N'Formato de brunch con pancakes y complementos pensado para sesiones de bienvenida, workshops internos y activaciones de marca.', 36000, 1, 120, N'/assets/images/b2.png', 0, 1, 0),
        (6, 2, N'Almuerzo premium para eventos internos', N'Eventos empresariales', N'Opcion de alimentacion ejecutiva para jornadas extendidas, comites internos y encuentros con clientes.', 54000, 1, 75, N'/assets/images/b1.png', 1, 1, 0),
        (7, 3, N'Estacion de frutas frescas para oficina', N'Bienestar corporativo', N'Servicio de fruta fresca presentada para recepciones, oficinas de alta rotacion y programas internos de bienestar.', 28000, 1, 80, N'/assets/images/b4.png', 1, 1, 0),
        (8, 3, N'Bowl funcional para jornadas de trabajo', N'Alimentacion saludable', N'Bowl ligero con ingredientes frescos pensado para reuniones, sesiones creativas y equipos operativos.', 43000, 1, 65, N'/assets/images/pl-12.png', 1, 1, 0),
        (9, 3, N'Menu saludable para comite ejecutivo', N'Wellness corporativo', N'Preparacion balanceada para atencion empresarial, pausas activas y espacios de bienestar organizacional.', 48000, 1, 55, N'/assets/images/pl-1.png', 0, 1, 0);

    SET IDENTITY_INSERT dbo.Products OFF;
END;
GO
