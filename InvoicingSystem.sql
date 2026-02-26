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
    Description NVARCHAR(MAX) NOT NULL, -- Corregido para coincidir con [Required]
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

-- 2. INSERCIÓN DE DATOS VARIADOS

-- Tipos de IVA
DECLARE @IVA21 UNIQUEIDENTIFIER = NEWID(), @IVA10 UNIQUEIDENTIFIER = NEWID(), @IVA04 UNIQUEIDENTIFIER = NEWID(), @IVA00 UNIQUEIDENTIFIER = NEWID();
INSERT INTO TaxRates (TaxRateId, Name, Percentage) VALUES 
(@IVA21, 'IVA 21%', 21.00), (@IVA10, 'IVA 10%', 10.00), (@IVA04, 'IVA 4%', 4.00), (@IVA00, 'IVA Exento', 0.00);

-- Condiciones de Pago
DECLARE @P30 UNIQUEIDENTIFIER = NEWID(), @P60 UNIQUEIDENTIFIER = NEWID(), @P90 UNIQUEIDENTIFIER = NEWID();
INSERT INTO PaymentTerms (PaymentTermsId, Description, PaymentDays) VALUES 
(@P30, 'Confirming 30 dias', 30), (@P60, 'Confirming 60 dias', 60),(@P90, 'Confirming 90 días', 90);

-- Clientes
INSERT INTO Customers (CustomerId, Name, Address, City, Nif) VALUES 
('CU2002-00182', 'SOCIEDAD MERCANTIL ESTATAL AGUAS DE LAS CUENCAS DE ESPAÑA', 'Calle Agustín de Betancourt, 25', 'Madrid', 'A50736784'),
('CU2026-00001', 'HARDWARE & SOFTWARE SOLUTIONS S.L.', 'Avenida de la Innovación, 12', 'Leganés', 'B88776655'),
('CU2026-00002', 'GLOBAL TECH RETAIL', 'Gran Vía 45', 'Madrid', 'B11223344');

-- Productos
DECLARE @Prod1 UNIQUEIDENTIFIER = NEWID(), @Prod2 UNIQUEIDENTIFIER = NEWID(), @Prod3 UNIQUEIDENTIFIER = NEWID(), @Prod4 UNIQUEIDENTIFIER = NEWID(), @Prod5 UNIQUEIDENTIFIER = NEWID();
INSERT INTO Products (ProductId, Name, Description, CurrentPrice) VALUES 
(@Prod1, 'IMPRESORA CANON MAXIFY BX110', 'Inyección de tinta profesional', 250.00),
(@Prod2, 'ORDENADOR HP ELITEDESK', 'Intel i7, 16GB RAM', 675.00),
(@Prod3, 'SERVIDOR DELL POWEREDGE', 'Xeon 12-Core, 32GB RAM', 3200.00),
(@Prod4, 'LICENCIA WINDOWS SERVER 2022', 'Licencia OEM standard', 450.00),
(@Prod5, 'HORA CONSULTORÍA IT', 'Soporte técnico avanzado', 65.00);

-- FACTURA A (La del PDF original)
INSERT INTO SalesInvoiceHeaders VALUES ('FA2602-2151', 'Impresora Canon MAXIFY', '2026-02-16', '2026-04-17', 'PR2602-2258', 'CU2002-00182', @P60);
INSERT INTO SalesInvoiceLines (SalesInvoiceHeaderId, ProductId, TaxRateId, UnitPrice, Quantity, CustomDescription) VALUES 
('FA2602-2151', @Prod1, @IVA21, 250.00, 1, '• S/N: 917069C02692AB21AHLX00737'),
('FA2602-2151', @Prod2, @IVA21, 675.00, 2, '• Color Gris' + CHAR(13) + '• Instalación incluida');

-- FACTURA B (Servicios y Software)
INSERT INTO SalesInvoiceHeaders VALUES ('FA2602-2152', 'Proyecto Migración Cloud', '2026-02-20', '2026-03-22', 'PR2602-9999', 'CU2026-00001', @P30);
INSERT INTO SalesInvoiceLines (SalesInvoiceHeaderId, ProductId, TaxRateId, UnitPrice, Quantity, CustomDescription) VALUES 
('FA2602-2152', @Prod4, @IVA21, 450.00, 1, '• Clave de activación por email'),
('FA2602-2152', @Prod5, @IVA21, 65.00, 10, '• Migración de base de datos SQL');

-- FACTURA C (Gran Volumen - Para probar saltos de página)
INSERT INTO SalesInvoiceHeaders VALUES ('FA2602-2153', 'Equipamiento Oficina Nueva', '2026-02-25', '2026-06-25', 'PR2602-5555', 'CU2026-00002', @P90);
INSERT INTO SalesInvoiceLines (SalesInvoiceHeaderId, ProductId, TaxRateId, UnitPrice, Quantity, CustomDescription) VALUES 
('FA2602-2153', @Prod3, @IVA21, 3200.00, 2, '• Configuración RAID 10' + CHAR(13) + '• Garantía 5 años ProSupport'),
('FA2602-2153', @Prod2, @IVA21, 675.00, 5, '• Monitor 24 pulgadas incluido');



