DROP TABLE IF EXISTS OrderItems CASCADE;
DROP TABLE IF EXISTS Orders CASCADE;
DROP TABLE IF EXISTS CustomerSessions CASCADE;
DROP TABLE IF EXISTS BusinessSessions CASCADE;
DROP TABLE IF EXISTS Products CASCADE;
DROP TABLE IF EXISTS Customers CASCADE;
DROP TABLE IF EXISTS Businesses CASCADE;

CREATE TABLE Businesses
(
    BusinessId INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    Slug VARCHAR(120) NOT NULL UNIQUE,
    OwnerName VARCHAR(120) NOT NULL,
    BusinessName VARCHAR(160) NOT NULL,
    Email VARCHAR(160) NOT NULL UNIQUE,
    PasswordHash VARCHAR(255) NOT NULL,
    PasswordSalt VARCHAR(255) NOT NULL,
    Phone VARCHAR(40) NOT NULL,
    City VARCHAR(80) NOT NULL,
    Address VARCHAR(220) NOT NULL,
    Tagline VARCHAR(180) NOT NULL,
    Description VARCHAR(900) NOT NULL,
    ShippingLeadTime VARCHAR(180) NOT NULL,
    MinimumOrderAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    LogoUrl VARCHAR(255) NOT NULL,
    BannerUrl VARCHAR(255) NOT NULL,
    WebsiteUrl VARCHAR(255) NOT NULL DEFAULT '',
    CreatedAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE Customers
(
    CustomerId INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    FullName VARCHAR(160) NOT NULL,
    Email VARCHAR(160) NOT NULL UNIQUE,
    PasswordHash VARCHAR(255) NOT NULL,
    PasswordSalt VARCHAR(255) NOT NULL,
    Phone VARCHAR(40) NOT NULL,
    City VARCHAR(80) NOT NULL,
    Address VARCHAR(220) NOT NULL,
    AuthProvider VARCHAR(40) NOT NULL DEFAULT 'password',
    CreatedAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE BusinessSessions
(
    BusinessSessionId INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    BusinessId INT NOT NULL,
    SessionToken VARCHAR(120) NOT NULL UNIQUE,
    ExpiresAt TIMESTAMP NOT NULL,
    CreatedAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT FK_BusinessSessions_Businesses FOREIGN KEY (BusinessId) REFERENCES Businesses(BusinessId)
);

CREATE TABLE CustomerSessions
(
    CustomerSessionId INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    CustomerId INT NOT NULL,
    SessionToken VARCHAR(120) NOT NULL UNIQUE,
    ExpiresAt TIMESTAMP NOT NULL,
    CreatedAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT FK_CustomerSessions_Customers FOREIGN KEY (CustomerId) REFERENCES Customers(CustomerId)
);

CREATE TABLE Products
(
    ProductId INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    BusinessId INT NOT NULL,
    Name VARCHAR(160) NOT NULL,
    Category VARCHAR(100) NOT NULL,
    Description VARCHAR(700) NOT NULL,
    Price DECIMAL(18,2) NOT NULL,
    MinimumOrder INT NOT NULL,
    Stock INT NOT NULL,
    ImageUrl VARCHAR(255) NOT NULL,
    IsFeatured BOOLEAN NOT NULL DEFAULT FALSE,
    IsPublished BOOLEAN NOT NULL DEFAULT TRUE,
    IsArchived BOOLEAN NOT NULL DEFAULT FALSE,
    CreatedAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT FK_Products_Businesses FOREIGN KEY (BusinessId) REFERENCES Businesses(BusinessId)
);

CREATE TABLE Orders
(
    OrderId INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    CustomerId INT NULL,
    BusinessId INT NULL,
    CustomerFullName VARCHAR(160) NOT NULL,
    CustomerEmail VARCHAR(160) NOT NULL,
    CustomerPhone VARCHAR(40) NOT NULL,
    CustomerCity VARCHAR(80) NOT NULL,
    DeliveryAddress VARCHAR(220) NOT NULL,
    Notes VARCHAR(500) NULL,
    Status VARCHAR(40) NOT NULL,
    IsNew BOOLEAN NOT NULL DEFAULT TRUE,
    ViewedAt TIMESTAMP NULL,
    Total DECIMAL(18,2) NOT NULL,
    PaymentMethod VARCHAR(80) NOT NULL DEFAULT 'No especificado',
    CreatedAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT FK_Orders_Businesses FOREIGN KEY (BusinessId) REFERENCES Businesses(BusinessId),
    CONSTRAINT FK_Orders_Customers FOREIGN KEY (CustomerId) REFERENCES Customers(CustomerId)
);

CREATE TABLE OrderItems
(
    OrderItemId INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    OrderId INT NOT NULL,
    ProductId INT NOT NULL,
    Quantity INT NOT NULL,
    UnitPrice DECIMAL(18,2) NOT NULL,
    LineTotal DECIMAL(18,2) NOT NULL,
    CONSTRAINT FK_OrderItems_Orders FOREIGN KEY (OrderId) REFERENCES Orders(OrderId),
    CONSTRAINT FK_OrderItems_Products FOREIGN KEY (ProductId) REFERENCES Products(ProductId)
);

CREATE INDEX IX_Products_BusinessId ON Products (BusinessId);
CREATE INDEX IX_Products_PublishedFeatured ON Products (IsArchived, IsPublished, IsFeatured);
CREATE INDEX IX_Orders_BusinessId ON Orders (BusinessId);
CREATE INDEX IX_Orders_CustomerId ON Orders (CustomerId);

INSERT INTO Businesses (Slug, OwnerName, BusinessName, Email, PasswordHash, PasswordSalt, Phone, City, Address, Tagline, Description, ShippingLeadTime, MinimumOrderAmount, LogoUrl, BannerUrl, WebsiteUrl)
VALUES
('andes-pack-studio', 'Mariana Cardenas', 'Andes Pack Studio', 'contacto@andespack.co', 'eulOwHJN9Z8F4/GGk74KA2QM/ddudtUGEGyUlsDrrfM=', 'b/GV47uErm89GXuh1mbVaw==', '+57 604 322 4410', 'Medellin', 'Cra. 43A #18 Sur-135, El Poblado, Medellin', 'Packaging corporativo reutilizable para marcas que quieren vender mejor.', 'Empresa especializada en empaques premium, textiles reutilizables y presentaciones corporativas para marcas que necesitan elevar su experiencia de entrega sin perder eficiencia operativa.', 'Entregas en Medellin en 24 horas y despachos nacionales entre 2 y 4 dias habiles.', 30000, '/assets/images/store2.png', '/assets/images/banner-andes-pack-studio.jpg', 'https://andespack.co'),
('aura-cafe-ejecutivo', 'Valentina Ruiz', 'Aura Cafe Ejecutivo', 'direccion@auracafe.co', 'oOGs3jemViisBU5N5jLxvi9wtw5E4gXSON91orrQfcc=', 'r+TMMNXdUFQc5ldz9YQ0Og==', '+57 601 745 8890', 'Bogota', 'Calle 85 #12-36, Chapinero, Bogota', 'Coffee breaks, desayunos y hospitalidad ejecutiva para reuniones, eventos y equipos comerciales.', 'Firma enfocada en experiencias de hospitalidad para oficinas y eventos empresariales, con formatos de coffee break, brunch corporativo y atencion alimentaria para jornadas comerciales o institucionales.', 'Cobertura en Bogota el mismo dia y envios nacionales entre 24 y 72 horas.', 45000, '/assets/images/store1.png', '/assets/images/banner-aura-cafe-ejecutivo.jpg', 'https://auracafe.co'),
('lumen-verde-bienestar', 'Sergio Latorre', 'Lumen Verde Bienestar', 'comercial@lumenverde.co', 'Q9dCftfYnDkfmhmFSPClZht4QtElQ+o207SS/LF3EnI=', 'hrMv67mhwOkZr2FA9edzfA==', '+57 602 485 6612', 'Cali', 'Avenida 6N #28N-45, Granada, Cali', 'Bienestar corporativo con estaciones saludables, bowls frescos y soluciones de alimentacion ligera.', 'Equipo especializado en experiencias de bienestar para empresas, con propuestas de fruta fresca, bowls funcionales y menus ligeros para jornadas internas, recepciones y espacios colaborativos.', 'Produccion entre 24 y 48 horas segun inventario y cobertura nacional desde Cali.', 35000, '/assets/images/store1.png', '/assets/images/banner-lumen-verde-bienestar.jpg', 'https://lumenverde.co');

INSERT INTO Products (BusinessId, Name, Category, Description, Price, MinimumOrder, Stock, ImageUrl, IsFeatured, IsPublished, IsArchived)
VALUES
(1, 'Impresora termica para etiquetado logistico', 'Empaques corporativos', 'Equipo compacto para imprimir etiquetas de despacho, referencias internas y control de inventario en operaciones comerciales.', 389000, 1, 24, '/assets/images/pla1.png', TRUE, TRUE, FALSE),
(1, 'Rollo premium de etiquetas adhesivas', 'Empaques sostenibles', 'Consumible para procesos de alistamiento, trazabilidad de pedidos y presentacion profesional del empaque.', 42000, 1, 220, '/assets/images/pla2.png', FALSE, TRUE, FALSE),
(1, 'Lector de codigo de barras para bodega', 'Presentacion de marca', 'Solucion de lectura rapida para control de inventario, despacho y validacion de referencias en puntos de empaque.', 165000, 1, 90, '/assets/images/pla3.png', TRUE, TRUE, FALSE),
(2, 'Coffee break ejecutivo para reuniones', 'Hospitalidad empresarial', 'Montaje de desayuno corporativo con pasteleria ligera, fruta y bebidas para juntas de direccion o visitas comerciales.', 69000, 1, 60, '/assets/images/breakfast.png', TRUE, TRUE, FALSE),
(2, 'Brunch artesanal para onboarding', 'Desayunos corporativos', 'Formato de brunch con pancakes y complementos pensado para sesiones de bienvenida, workshops internos y activaciones de marca.', 36000, 1, 120, '/assets/images/b2.png', FALSE, TRUE, FALSE),
(2, 'Almuerzo premium para eventos internos', 'Eventos empresariales', 'Opcion de alimentacion ejecutiva para jornadas extendidas, comites internos y encuentros con clientes.', 54000, 1, 75, '/assets/images/b1.png', TRUE, TRUE, FALSE),
(3, 'Estacion de frutas frescas para oficina', 'Bienestar corporativo', 'Servicio de fruta fresca presentada para recepciones, oficinas de alta rotacion y programas internos de bienestar.', 28000, 1, 80, '/assets/images/b4.png', TRUE, TRUE, FALSE),
(3, 'Bowl funcional para jornadas de trabajo', 'Alimentacion saludable', 'Bowl ligero con ingredientes frescos pensado para reuniones, sesiones creativas y equipos operativos.', 43000, 1, 65, '/assets/images/pl-12.png', TRUE, TRUE, FALSE),
(3, 'Menu saludable para comite ejecutivo', 'Wellness corporativo', 'Preparacion balanceada para atencion empresarial, pausas activas y espacios de bienestar organizacional.', 48000, 1, 55, '/assets/images/pl-1.png', FALSE, TRUE, FALSE);
