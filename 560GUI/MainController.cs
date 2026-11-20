using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using Microsoft.Data.SqlClient;


namespace _560GUI
{
    public class MainController
    {

        //SqlConnection connection;

        //public MainController()
        //{
        //    connection = new("Data Source=(localdb)\\MSSQLLocalDb;Integrated Security=True;Persist Security Info=False;Pooling=False;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Application Name=\"SQL Server Management Studio\";Command Timeout=0;Initial Catalog=CIS560");
        //}

        //public async Task<object[,]> TestQuery()
        //{
        //    object[,] data = new object[10,10];
        //    string q = "SELECT * FROM Clubs.Club;";
        //    using( SqlCommand command = new(q,connection))
        //    {
        //        connection.Open();
        //        using(SqlDataReader reader = command.ExecuteReader())
        //        {
        //            int row = 0;
        //            while(reader.Read())
        //            {
        //                for (int i = 0; i < 5; i++)
        //                {
        //                    data[row,i] = reader.GetValue(i);
        //                }
        //                row++;
        //            }
        //        }
        //    }
        //    return data;
        //}

        private const string ConnectionString =
            @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=CIS560;Integrated Security=True;TrustServerCertificate=True";


        /// <summary>
        /// Runs a stored procedure (isProc = true) or a plain SQL statement and returns the rows.
        /// A new connection is opened and closed on every call.
        /// </summary>
        public async Task<DataTable> RunAsync(string sqlOrProc, bool isProc, params SqlParameter[] parameters)
        {
            using var connection = new SqlConnection(ConnectionString);
            using var command = new SqlCommand(sqlOrProc, connection)
            {
                CommandType = isProc ? CommandType.StoredProcedure : CommandType.Text
            };
            command.Parameters.AddRange(parameters);
            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            var table = new DataTable();
            table.Load(reader);
            return table;
        }


        public Task<DataTable> GetCustomers() =>
            RunAsync("SELECT CustomerID, CustomerName, AccountOpenedDate FROM CIS560.Customers ORDER BY CustomerName;", false);

        public Task<DataTable> GetSuppliers() =>
            RunAsync("SELECT * FROM CIS560.Suppliers ORDER BY SupplierName;", false);

        // ---- Reports (stored procedures from Queries.sql) ----

        /// <summary>
        /// Creates a DATE parameter for a stored procedure call.
        /// </summary>
        private static SqlParameter DateParam(string name, DateTime value)
        {
            SqlParameter parameter = new SqlParameter(name, SqlDbType.Date);
            parameter.Value = value;
            return parameter;
        }

        public Task<DataTable> GetProductSales(DateTime start, DateTime stop)
        {
            SqlParameter startParam = DateParam("@StartDate", start);
            SqlParameter stopParam = DateParam("@StopDate", stop);
            return RunAsync("CIS560.GetProductSales", true, startParam, stopParam);
        }

        public Task<DataTable> GetTopCustomers(DateTime start, DateTime stop)
        {
            SqlParameter startParam = DateParam("@StartDate", start);
            SqlParameter stopParam = DateParam("@StopDate", stop);
            return RunAsync("CIS560.GetTopCustomers", true, startParam, stopParam);
        }

        public Task<DataTable> GetBestSellingProductsBySupplier(DateTime start, DateTime stop)
        {
            SqlParameter startParam = DateParam("@StartDate", start);
            SqlParameter stopParam = DateParam("@StopDate", stop);
            return RunAsync("CIS560.GetBestSellingProductsBySupplier", true, startParam, stopParam);
        }

        public Task<DataTable> GetMonthlyProductTypeSales(DateTime end)
        {
            SqlParameter endParam = DateParam("@EndDate", end);
            return RunAsync("CIS560.GetMonthlyProductTypeSales", true, endParam);
        }

        /// <summary>
        /// Connection check: lists every product with its supplier, type, and brand.
        /// </summary>
        public Task<DataTable> TestQuery()
        {
            const string q = @"
                SELECT P.ProductID, P.ProductName, T.ProductTypeName AS ProductType,
                       B.BrandName AS Brand, S.SupplierName AS Supplier,
                       P.QuantityOnHand, P.BulkPrice, P.SellPrice
                FROM CIS560.Products P
                    INNER JOIN CIS560.ProductTypes T ON T.ProductTypeID = P.ProductTypeID
                    INNER JOIN CIS560.Brands B       ON B.BrandID       = P.BrandID
                    INNER JOIN CIS560.Suppliers S    ON S.SupplierID    = P.SupplierID
                ORDER BY P.ProductName;";
            return RunAsync(q, false);
        }
    }
}
