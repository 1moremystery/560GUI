USE CIS560;
GO

--6.1: Top Selling Products
-- Shows the quantity sold and total profit for each product grouped by year and month.


CREATE OR ALTER PROCEDURE CIS560.GetProductSales
    @StartDate DATE,
    @StopDate DATE
AS
BEGIN
    SELECT P.ProductID, P.ProductName, YEAR(CSO.OrderDate) AS OrderYear, MONTH(CSO.OrderDate) AS OrderMonth, SUM(CSOL.Quantity) AS QuantitySold,
SUM(CSOL.Quantity * CSOL.UnitPrice) AS TotalSales
    FROM CIS560.Products P
    INNER JOIN CIS560.CustomerSalesOrderLines CSOL ON P.ProductID = CSOL.ProductID
    INNER JOIN CIS560.CustomerSalesOrder CSO ON CSOL.CustomerSalesOrderID = CSO.CustomerSalesOrderID
    WHERE CSO.OrderDate >= @StartDate AND CSO.OrderDate <= @StopDate
    GROUP BY P.ProductID, P.ProductName, YEAR(CSO.OrderDate), MONTH(CSO.OrderDate)
    ORDER BY TotalSales DESC;
END;
GO

--6.2: Top Ten Customers and Purchases
-- Shows the top ten customers with the most orders and
-- displays each customer's most commonly purchased product.

CREATE OR ALTER PROCEDURE CIS560.GetTopCustomers
    @StartDate DATE,
    @StopDate DATE
AS
BEGIN
    WITH CustomerOrderCounts AS
    (
        SELECT C.CustomerID, C.CustomerName, C.AccountOpenedDate,
            COUNT(CSO.CustomerSalesOrderID) AS TotalOrders
        FROM CIS560.Customers C
        INNER JOIN CIS560.CustomerSalesOrder CSO ON C.CustomerID = CSO.CustomerID
        WHERE CSO.OrderDate >= @StartDate  AND CSO.OrderDate <= @StopDate
        GROUP BY  C.CustomerID, C.CustomerName, C.AccountOpenedDate
    ),
    CustomerProductTotals AS
    (
        SELECT  CSO.CustomerID,  P.ProductID,  P.ProductName,
            SUM(CSOL.Quantity) AS QuantityPurchased
        FROM CIS560.CustomerSalesOrder CSO
        INNER JOIN CIS560.CustomerSalesOrderLines CSOL
            ON CSO.CustomerSalesOrderID = CSOL.CustomerSalesOrderID
        INNER JOIN CIS560.Products P ON CSOL.ProductID = P.ProductID
        WHERE CSO.OrderDate >= @StartDate AND CSO.OrderDate <= @StopDate
        GROUP BY CSO.CustomerID, P.ProductID, P.ProductName
    ),
    RankedProducts AS
    (
        SELECT CustomerID, ProductName, QuantityPurchased,
            ROW_NUMBER() OVER (
                PARTITION BY CustomerID
                ORDER BY QuantityPurchased DESC, ProductName
            ) AS ProductRank
        FROM CustomerProductTotals
    ),
    TopCustomers AS
    (
        SELECT CustomerID, CustomerName, AccountOpenedDate, TotalOrders,
            ROW_NUMBER() OVER (
                ORDER BY TotalOrders DESC, CustomerID
            ) AS CustomerRank
        FROM CustomerOrderCounts
    )
    SELECT TC.CustomerID, TC.CustomerName, TC.AccountOpenedDate, TC.TotalOrders,
        RP.ProductName AS CommonlyPurchasedProduct,  RP.QuantityPurchased
    FROM TopCustomers TC
    INNER JOIN RankedProducts RP ON TC.CustomerID = RP.CustomerID
    WHERE TC.CustomerRank <= 10 AND RP.ProductRank = 1
    ORDER BY TC.TotalOrders DESC;
END;
GO


--6.3: Best Selling Products by Supplier
-- Finds the best-selling product for each supplier during the specified date range.
CREATE OR ALTER PROCEDURE CIS560.GetBestSellingProductsBySupplier
    @StartDate DATE,
    @StopDate DATE
AS
BEGIN
    WITH SupplierProductSales AS
    (
        SELECT S.SupplierID, S.SupplierName, P.ProductID, P.ProductName,
            P.SellPrice AS ProductPrice, SUM(CSOL.Quantity) AS TotalProductsSold,
            SUM(CSOL.Quantity * CSOL.UnitPrice) AS TotalProductSales
        FROM CIS560.Suppliers S  INNER JOIN CIS560.Products P ON S.SupplierID = P.SupplierID
        INNER JOIN CIS560.CustomerSalesOrderLines CSOL ON P.ProductID = CSOL.ProductID
        INNER JOIN CIS560.CustomerSalesOrder CSO ON CSOL.CustomerSalesOrderID = CSO.CustomerSalesOrderID
        WHERE CSO.OrderDate >= @StartDate  AND CSO.OrderDate <= @StopDate
        GROUP BY S.SupplierID, S.SupplierName, P.ProductID, P.ProductName, P.SellPrice
    ),
    RankedSupplierProducts AS
    (
        SELECT SupplierID, SupplierName, TotalProductsSold, ProductName, ProductID,
            ProductPrice, TotalProductSales,
            ROW_NUMBER() OVER (
                PARTITION BY SupplierID
                ORDER BY TotalProductSales DESC, ProductID
            ) AS ProductRank
        FROM SupplierProductSales
    )
    SELECT SupplierID, SupplierName, TotalProductsSold, ProductName, ProductID,
        ProductPrice, TotalProductSales
    FROM RankedSupplierProducts WHERE ProductRank = 1
    ORDER BY TotalProductSales DESC;
END;
GO


--6.4: Product Popularity, Month to Month
-- Shows monthly profit and quantity sold for every product type
-- over the twelve calendar months ending with @EndDate.
-- Ranks product types within each month by total sales revenue.

CREATE OR ALTER PROCEDURE CIS560.GetMonthlyProductTypeSales
    @EndDate DATE
AS
BEGIN
    DECLARE @FirstMonth DATE;

    SET @FirstMonth =
        DATEADD(MONTH, -11, DATEFROMPARTS(YEAR(@EndDate), MONTH(@EndDate), 1));

    WITH MonthList AS
    (
        SELECT @FirstMonth AS MonthStart
        UNION ALL
        SELECT DATEADD(MONTH, 1, MonthStart)
        FROM MonthList
        WHERE MonthStart <
            DATEFROMPARTS(YEAR(@EndDate), MONTH(@EndDate), 1)
    ),
    MonthlySales AS
    (
        SELECT PT.ProductTypeID,
            YEAR(CSO.OrderDate) AS OrderYear,
            MONTH(CSO.OrderDate) AS OrderMonth,
            SUM(CSOL.Quantity * CSOL.UnitPrice) AS TotalSales,
            SUM(CSOL.Quantity) AS QuantitySold
        FROM CIS560.ProductTypes PT
        INNER JOIN CIS560.Products P ON PT.ProductTypeID = P.ProductTypeID
        INNER JOIN CIS560.CustomerSalesOrderLines CSOL  ON P.ProductID = CSOL.ProductID
        INNER JOIN CIS560.CustomerSalesOrder CSO ON CSOL.CustomerSalesOrderID = CSO.CustomerSalesOrderID
        WHERE CSO.OrderDate >= @FirstMonth AND CSO.OrderDate <= @EndDate
        GROUP BY PT.ProductTypeID,  YEAR(CSO.OrderDate),  MONTH(CSO.OrderDate)
    ),
    ProductTypeMonths AS
    (
        SELECT PT.ProductTypeID, PT.ProductTypeName,
            YEAR(ML.MonthStart) AS OrderYear,
            MONTH(ML.MonthStart) AS OrderMonth,
            ISNULL(MS.TotalSales, 0) AS TotalSales,
            ISNULL(MS.QuantitySold, 0) AS QuantitySold
        FROM CIS560.ProductTypes PT
        CROSS JOIN MonthList ML
        LEFT JOIN MonthlySales MS
            ON PT.ProductTypeID = MS.ProductTypeID
            AND YEAR(ML.MonthStart) = MS.OrderYear
            AND MONTH(ML.MonthStart) = MS.OrderMonth
    ),
    RankedProductTypes AS
    (
        SELECT ProductTypeName, TotalSales, OrderMonth,  OrderYear, QuantitySold,
            RANK() OVER (
                PARTITION BY OrderYear, OrderMonth
                ORDER BY TotalSales DESC
            ) AS MonthlyRank
        FROM ProductTypeMonths
    )
    SELECT
        ProductTypeName AS ProductType, TotalSales, OrderMonth, OrderYear, QuantitySold, MonthlyRank
    FROM RankedProductTypes
    ORDER BY OrderYear, OrderMonth, MonthlyRank, ProductType;
END;
