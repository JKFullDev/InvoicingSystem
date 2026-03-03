-- 1. ESTRUCTURA DE LA BASE DE DATOS
USE prueba
DROP DATABASE IF EXISTS InvoicingSystem;
CREATE DATABASE InvoicingSystem;
GO

-- 1. ESTRUCTURA (Manual, siguiendo la preferencia de tu equipo)
USE InvoicingSystem;
GO

-- Borrado en orden inverso de jerarquía para evitar errores de FK
DROP TABLE IF EXISTS SalesInvoiceLines;
DROP TABLE IF EXISTS SalesInvoiceHeaders;
DROP TABLE IF EXISTS TaxRates;
DROP TABLE IF EXISTS Products;
DROP TABLE IF EXISTS Customers;
DROP TABLE IF EXISTS PaymentTerms;
GO

CREATE TABLE PaymentTerms (
    PaymentTermsId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Description NVARCHAR(100) NOT NULL,
    PaymentDays INT NOT NULL
);

CREATE TABLE Customers (
    CustomerId NVARCHAR(50) PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,
    Address NVARCHAR(500) NOT NULL,
    City NVARCHAR(100) NOT NULL,
    Nif NVARCHAR(20) NOT NULL
);

CREATE TABLE Products (
    ProductId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX) NOT NULL, 
    CurrentPrice DECIMAL(18, 2) NOT NULL
);

CREATE TABLE TaxRates (
    TaxRateId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(50) NOT NULL,
    Percentage DECIMAL(5, 2) NOT NULL
);

CREATE TABLE SalesInvoiceHeaders (
    SalesInvoiceHeaderId NVARCHAR(50) PRIMARY KEY,
    CustomerReference NVARCHAR(100) NOT NULL,
    InvoiceDate DATE NOT NULL,
    DueDate DATE NOT NULL,
    QuoteReference NVARCHAR(100) NOT NULL,
    CustomerId NVARCHAR(50) NOT NULL,
    PaymentTermsId UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT FK_Invoices_Customers FOREIGN KEY (CustomerId) REFERENCES Customers(CustomerId),
    CONSTRAINT FK_Invoices_PaymentTerms FOREIGN KEY (PaymentTermsId) REFERENCES PaymentTerms(PaymentTermsId)
);

CREATE TABLE SalesInvoiceLines (
    SalesInvoiceLineId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    SalesInvoiceHeaderId NVARCHAR(50) NOT NULL,
    ProductId UNIQUEIDENTIFIER NOT NULL,
    TaxRateId UNIQUEIDENTIFIER NOT NULL,
    UnitPrice DECIMAL(18, 2) NOT NULL,
    Quantity INT NOT NULL,
    CustomDescription NVARCHAR(MAX) NOT NULL,
    CONSTRAINT FK_Lines_Headers FOREIGN KEY (SalesInvoiceHeaderId) REFERENCES SalesInvoiceHeaders(SalesInvoiceHeaderId),
    CONSTRAINT FK_Lines_Products FOREIGN KEY (ProductId) REFERENCES Products(ProductId),
    CONSTRAINT FK_Lines_TaxRates FOREIGN KEY (TaxRateId) REFERENCES TaxRates(TaxRateId)
);
GO

SET NOCOUNT ON;

PRINT '1. LIMPIANDO TABLAS (Sin borrar la DB)...';
ALTER TABLE SalesInvoiceLines NOCHECK CONSTRAINT ALL;
ALTER TABLE SalesInvoiceHeaders NOCHECK CONSTRAINT ALL;

DELETE FROM SalesInvoiceLines;
DELETE FROM SalesInvoiceHeaders;
DELETE FROM Customers;
DELETE FROM Products;
DELETE FROM TaxRates;
DELETE FROM PaymentTerms;

-- Reactivamos restricciones
ALTER TABLE SalesInvoiceLines CHECK CONSTRAINT ALL;
ALTER TABLE SalesInvoiceHeaders CHECK CONSTRAINT ALL;
GO

PRINT '2. INSERTANDO DATOS MAESTROS (Esenciales para que no falle)...';

-- Tipos de IVA (Guardamos IDs en variables tabla para usarlos luego)
DECLARE @TaxMap TABLE (Id UNIQUEIDENTIFIER, Pct DECIMAL(5,2));
INSERT INTO TaxRates (TaxRateId, Name, Percentage) 
OUTPUT Inserted.TaxRateId, Inserted.Percentage INTO @TaxMap
VALUES 
(NEWID(), 'IVA 21%', 21.00), 
(NEWID(), 'IVA 10%', 10.00), 
(NEWID(), 'IVA 4%', 4.00), 
(NEWID(), 'Exento', 0.00);

-- Condiciones de Pago
DECLARE @PayMap TABLE (Id UNIQUEIDENTIFIER, Days INT);
INSERT INTO PaymentTerms (PaymentTermsId, Description, PaymentDays) 
OUTPUT Inserted.PaymentTermsId, Inserted.PaymentDays INTO @PayMap
VALUES 
(NEWID(), 'Contado', 0), 
(NEWID(), 'Giro 30 días', 30), 
(NEWID(), 'Giro 60 días', 60);

PRINT '3. GENERANDO 50 CLIENTES...';
DECLARE @i INT = 1;
WHILE @i <= 50
BEGIN
    DECLARE @CId NVARCHAR(50) = 'CL-' + RIGHT('0000' + CAST(@i AS VARCHAR(10)), 4);
    
    INSERT INTO Customers (CustomerId, Name, Address, City, Nif)
    VALUES (
        @CId,
        'Empresa ' + CAST(@i AS VARCHAR(10)) + ' ' + CHAR(65 + (@i % 26)) + 'L', 
        'Calle Falsa ' + CAST(ABS(CHECKSUM(NEWID()) % 100) AS VARCHAR(10)),
        CASE ABS(CHECKSUM(NEWID()) % 4) 
            WHEN 0 THEN 'Madrid' WHEN 1 THEN 'Barcelona' WHEN 2 THEN 'Valencia' ELSE 'Sevilla' 
        END,
        'B' + RIGHT('00000000' + CAST(ABS(CHECKSUM(NEWID())) AS VARCHAR(20)), 8)
    );
    SET @i = @i + 1;
END

PRINT '4. GENERANDO 50 PRODUCTOS...';
SET @i = 1;
WHILE @i <= 50
BEGIN
    INSERT INTO Products (ProductId, Name, Description, CurrentPrice)
    VALUES (
        NEWID(),
        'Producto IT ' + CAST(@i AS VARCHAR(10)),
        'Descripción autogenerada del producto ' + CAST(@i AS VARCHAR(10)),
        CAST((ABS(CHECKSUM(NEWID()) % 10000) / 10.0) + 10 AS DECIMAL(18,2))
    );
    SET @i = @i + 1;
END

PRINT '5. GENERANDO 500 FACTURAS (Esto tardará unos segundos)...';
SET @i = 1;

-- Variables para el bucle
DECLARE @NewInvoiceId NVARCHAR(50);
DECLARE @CustId NVARCHAR(50);
DECLARE @PayTermId UNIQUEIDENTIFIER;
DECLARE @PayDays INT;
DECLARE @InvDate DATE;
DECLARE @ProdId UNIQUEIDENTIFIER;
DECLARE @Price DECIMAL(18,2);
DECLARE @TaxId UNIQUEIDENTIFIER;

WHILE @i <= 500
BEGIN
    -- Datos aleatorios para Cabecera
    SET @NewInvoiceId = 'F24-' + RIGHT('00000' + CAST(@i AS VARCHAR(10)), 5);
    
    SELECT TOP 1 @CustId = CustomerId FROM Customers ORDER BY NEWID();
    SELECT TOP 1 @PayTermId = Id, @PayDays = Days FROM @PayMap ORDER BY NEWID();
    
    SET @InvDate = DATEADD(DAY, -ABS(CHECKSUM(NEWID()) % 365), GETDATE()); -- Fecha aleatoria último año

    -- Insertar Cabecera
    INSERT INTO SalesInvoiceHeaders (SalesInvoiceHeaderId, CustomerReference, InvoiceDate, DueDate, QuoteReference, CustomerId, PaymentTermsId)
    VALUES (
        @NewInvoiceId,
        'REF-' + CAST(@i AS VARCHAR(10)),
        @InvDate,
        DATEADD(DAY, @PayDays, @InvDate), -- Vencimiento calculado
        'PRE-' + CAST(@i AS VARCHAR(10)),
        @CustId,
        @PayTermId
    );

    -- Insertar Líneas (Entre 1 y 3 líneas por factura)
    DECLARE @Lines INT = (ABS(CHECKSUM(NEWID())) % 3) + 1;
    DECLARE @j INT = 1;
    
    WHILE @j <= @Lines
    BEGIN
        SELECT TOP 1 @ProdId = ProductId, @Price = CurrentPrice FROM Products ORDER BY NEWID();
        SELECT TOP 1 @TaxId = Id FROM @TaxMap WHERE Pct = 21.00; -- Asumimos IVA 21 por defecto para simplificar

        INSERT INTO SalesInvoiceLines (SalesInvoiceHeaderId, ProductId, TaxRateId, UnitPrice, Quantity, CustomDescription)
        VALUES (
            @NewInvoiceId,
            @ProdId,
            @TaxId,
            @Price,
            (ABS(CHECKSUM(NEWID())) % 5) + 1,
            'Linea generada automáticamente'
        );
        SET @j = @j + 1;
    END

    SET @i = @i + 1;
END

PRINT '¡PROCESO COMPLETADO SIN ERRORES!';
GO