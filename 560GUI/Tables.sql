/* =====================================================================
   TTRPG Shop - Tables.sql
   Creates every table in the physical database model.
   Target: Microsoft SQL Server 2016 or later.

   Run order: this script drops existing tables (children first) and
   recreates them (parents first), so it can be re-run safely.
   ===================================================================== */

SET NOCOUNT ON;
GO

/* ---------------------------------------------------------------------
   Drop existing tables (child tables before parent tables)
   --------------------------------------------------------------------- */
DROP TABLE IF EXISTS dbo.CustomerSalesOrderLines;
DROP TABLE IF EXISTS dbo.CustomerSalesOrder;
DROP TABLE IF EXISTS dbo.PurchaseOrderLines;
DROP TABLE IF EXISTS dbo.PurchaseOrder;
DROP TABLE IF EXISTS dbo.Products;
DROP TABLE IF EXISTS dbo.Customers;
DROP TABLE IF EXISTS dbo.Brands;
DROP TABLE IF EXISTS dbo.ProductTypes;
DROP TABLE IF EXISTS dbo.Suppliers;
GO

/* ---------------------------------------------------------------------
   Suppliers - distributors the store buys products from
   --------------------------------------------------------------------- */
CREATE TABLE dbo.Suppliers
(
    SupplierID      INT IDENTITY(1,1)   NOT NULL,
    SupplierName    NVARCHAR(100)       NOT NULL,

    CONSTRAINT PK_Suppliers PRIMARY KEY (SupplierID),
    CONSTRAINT UK_Suppliers_SupplierName UNIQUE (SupplierName)
);
GO

/* ---------------------------------------------------------------------
   ProductTypes - category of product (Rulebook, Dice, Miniatures, ...)
   --------------------------------------------------------------------- */
CREATE TABLE dbo.ProductTypes
(
    ProductTypeID   INT IDENTITY(1,1)   NOT NULL,
    ProductTypeName NVARCHAR(50)        NOT NULL,

    CONSTRAINT PK_ProductTypes PRIMARY KEY (ProductTypeID),
    CONSTRAINT UK_ProductTypes_ProductTypeName UNIQUE (ProductTypeName)
);
GO

/* ---------------------------------------------------------------------
   Brands - publisher / manufacturer of a product (WotC, Paizo, ...)
   --------------------------------------------------------------------- */
CREATE TABLE dbo.Brands
(
    BrandID         INT IDENTITY(1,1)   NOT NULL,
    BrandName       NVARCHAR(100)       NOT NULL,

    CONSTRAINT PK_Brands PRIMARY KEY (BrandID),
    CONSTRAINT UK_Brands_BrandName UNIQUE (BrandName)
);
GO

/* ---------------------------------------------------------------------
   Customers - store account holders
   --------------------------------------------------------------------- */
CREATE TABLE dbo.Customers
(
    CustomerID          INT IDENTITY(1,1)   NOT NULL,
    CustomerName        NVARCHAR(100)       NOT NULL,
    AccountOpenedDate   DATE                NOT NULL
        CONSTRAINT DF_Customers_AccountOpenedDate DEFAULT (CAST(SYSDATETIME() AS DATE)),

    CONSTRAINT PK_Customers PRIMARY KEY (CustomerID),
    CONSTRAINT UK_Customers_CustomerName UNIQUE (CustomerName),
    CONSTRAINT CK_Customers_AccountOpenedDate
        CHECK (AccountOpenedDate <= CAST(SYSDATETIME() AS DATE))
);
GO

/* ---------------------------------------------------------------------
   Products - items the store stocks and sells.
   BulkPrice = current cost from the supplier.
   SellPrice = current retail price.
   Historical prices are kept on the order lines (UnitCost / UnitPrice).
   --------------------------------------------------------------------- */
CREATE TABLE dbo.Products
(
    ProductID       INT IDENTITY(1,1)   NOT NULL,
    SupplierID      INT                 NOT NULL,
    ProductTypeID   INT                 NOT NULL,
    BrandID         INT                 NOT NULL,
    ProductName     NVARCHAR(150)       NOT NULL,
    QuantityOnHand  INT                 NOT NULL
        CONSTRAINT DF_Products_QuantityOnHand DEFAULT (0),
    BulkPrice       DECIMAL(10,2)       NOT NULL,
    SellPrice       DECIMAL(10,2)       NOT NULL,

    CONSTRAINT PK_Products PRIMARY KEY (ProductID),
    CONSTRAINT UK_Products_ProductName UNIQUE (ProductName),
    CONSTRAINT FK_Products_Suppliers
        FOREIGN KEY (SupplierID) REFERENCES dbo.Suppliers (SupplierID),
    CONSTRAINT FK_Products_ProductTypes
        FOREIGN KEY (ProductTypeID) REFERENCES dbo.ProductTypes (ProductTypeID),
    CONSTRAINT FK_Products_Brands
        FOREIGN KEY (BrandID) REFERENCES dbo.Brands (BrandID),
    CONSTRAINT CK_Products_QuantityOnHand CHECK (QuantityOnHand >= 0),
    CONSTRAINT CK_Products_BulkPrice CHECK (BulkPrice >= 0),
    CONSTRAINT CK_Products_SellPrice CHECK (SellPrice >= 0)
);
GO

/* ---------------------------------------------------------------------
   PurchaseOrder - an order the store places with a supplier
   --------------------------------------------------------------------- */
CREATE TABLE dbo.PurchaseOrder
(
    PurchaseOrderID INT IDENTITY(1,1)   NOT NULL,
    SupplierID      INT                 NOT NULL,
    OrderDate       DATE                NOT NULL
        CONSTRAINT DF_PurchaseOrder_OrderDate DEFAULT (CAST(SYSDATETIME() AS DATE)),

    CONSTRAINT PK_PurchaseOrder PRIMARY KEY (PurchaseOrderID),
    CONSTRAINT FK_PurchaseOrder_Suppliers
        FOREIGN KEY (SupplierID) REFERENCES dbo.Suppliers (SupplierID)
);
GO

/* ---------------------------------------------------------------------
   PurchaseOrderLines - one product on a purchase order (weak entity).
   UnitCost records what the store paid at the time of the order.
   Lines are deleted automatically when their purchase order is deleted.
   --------------------------------------------------------------------- */
CREATE TABLE dbo.PurchaseOrderLines
(
    PurchaseOrderLinesID    INT IDENTITY(1,1)   NOT NULL,
    PurchaseOrderID         INT                 NOT NULL,
    ProductID               INT                 NOT NULL,
    Quantity                INT                 NOT NULL,
    UnitCost                DECIMAL(10,2)       NOT NULL,

    CONSTRAINT PK_PurchaseOrderLines PRIMARY KEY (PurchaseOrderLinesID),
    CONSTRAINT UK_PurchaseOrderLines_PurchaseOrderID_ProductID
        UNIQUE (PurchaseOrderID, ProductID),
    CONSTRAINT FK_PurchaseOrderLines_PurchaseOrder
        FOREIGN KEY (PurchaseOrderID) REFERENCES dbo.PurchaseOrder (PurchaseOrderID)
        ON DELETE CASCADE,
    CONSTRAINT FK_PurchaseOrderLines_Products
        FOREIGN KEY (ProductID) REFERENCES dbo.Products (ProductID),
    CONSTRAINT CK_PurchaseOrderLines_Quantity CHECK (Quantity > 0),
    CONSTRAINT CK_PurchaseOrderLines_UnitCost CHECK (UnitCost >= 0)
);
GO

/* ---------------------------------------------------------------------
   CustomerSalesOrder - a sale to a customer
   --------------------------------------------------------------------- */
CREATE TABLE dbo.CustomerSalesOrder
(
    CustomerSalesOrderID    INT IDENTITY(1,1)   NOT NULL,
    CustomerID              INT                 NOT NULL,
    OrderDate               DATE                NOT NULL
        CONSTRAINT DF_CustomerSalesOrder_OrderDate DEFAULT (CAST(SYSDATETIME() AS DATE)),

    CONSTRAINT PK_CustomerSalesOrder PRIMARY KEY (CustomerSalesOrderID),
    CONSTRAINT FK_CustomerSalesOrder_Customers
        FOREIGN KEY (CustomerID) REFERENCES dbo.Customers (CustomerID)
);
GO

/* ---------------------------------------------------------------------
   CustomerSalesOrderLines - one product on a sales order (weak entity).
   UnitPrice records what the customer paid at the time of the sale.
   Lines are deleted automatically when their sales order is deleted.
   --------------------------------------------------------------------- */
CREATE TABLE dbo.CustomerSalesOrderLines
(
    CustomerSalesOrderLinesID    INT IDENTITY(1,1)   NOT NULL,
    CustomerSalesOrderID    INT                 NOT NULL,
    ProductID               INT                 NOT NULL,
    Quantity                INT                 NOT NULL,
    UnitPrice               DECIMAL(10,2)       NOT NULL,

    CONSTRAINT PK_CustomerSalesOrderLines PRIMARY KEY (CustomerSalesOrderLinesID),
    CONSTRAINT UK_CustomerSalesOrderLines_CustomerSalesOrderID_ProductID
        UNIQUE (CustomerSalesOrderID, ProductID),
    CONSTRAINT FK_CustomerSalesOrderLines_CustomerSalesOrder
        FOREIGN KEY (CustomerSalesOrderID) REFERENCES dbo.CustomerSalesOrder (CustomerSalesOrderID)
        ON DELETE CASCADE,
    CONSTRAINT FK_CustomerSalesOrderLines_Products
        FOREIGN KEY (ProductID) REFERENCES dbo.Products (ProductID),
    CONSTRAINT CK_CustomerSalesOrderLines_Quantity CHECK (Quantity > 0),
    CONSTRAINT CK_CustomerSalesOrderLines_UnitPrice CHECK (UnitPrice >= 0)
);
GO

/* ---------------------------------------------------------------------
   Indexes on foreign key columns (speed up joins in reports).
   The UNIQUE constraints on the line tables already index the
   (OrderID, ProductID) pairs, so only ProductID needs its own index.
   --------------------------------------------------------------------- */
CREATE INDEX IX_Products_SupplierID          ON dbo.Products (SupplierID);
CREATE INDEX IX_Products_ProductTypeID       ON dbo.Products (ProductTypeID);
CREATE INDEX IX_Products_BrandID             ON dbo.Products (BrandID);
CREATE INDEX IX_PurchaseOrder_SupplierID     ON dbo.PurchaseOrder (SupplierID);
CREATE INDEX IX_PurchaseOrderLines_ProductID ON dbo.PurchaseOrderLines (ProductID);
CREATE INDEX IX_CustomerSalesOrder_CustomerID ON dbo.CustomerSalesOrder (CustomerID);
CREATE INDEX IX_CustomerSalesOrder_OrderDate ON dbo.CustomerSalesOrder (OrderDate);
CREATE INDEX IX_CustomerSalesOrderLines_ProductID ON dbo.CustomerSalesOrderLines (ProductID);
GO
