/*
    Migracion no destructiva para bases existentes.

    Agrega archivado logico de productos, usado por el backend para ocultar
    productos eliminados sin romper OrderItems ni el historial de pedidos.
*/

USE TiendaMicroempresas;
GO

IF COL_LENGTH(N'dbo.Products', N'IsArchived') IS NULL
BEGIN
    ALTER TABLE dbo.Products
    ADD IsArchived BIT NOT NULL
        CONSTRAINT DF_Products_IsArchived DEFAULT 0;
END;
GO

IF EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_Products_PublishedFeatured'
      AND object_id = OBJECT_ID(N'dbo.Products')
)
BEGIN
    DROP INDEX IX_Products_PublishedFeatured ON dbo.Products;
END;
GO

CREATE INDEX IX_Products_PublishedFeatured
ON dbo.Products (IsArchived, IsPublished, IsFeatured);
GO

