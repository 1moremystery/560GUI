using _560GUI.DataClasses;
using _560GUI.Dialogs;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;


namespace _560GUI
{
    public class MainController
    {

        SqlConnection connection;

        public MainController()
        {
            connection = new("Data Source=(localdb)\\MSSQLLocalDb;Integrated Security=True;Persist Security Info=False;Pooling=False;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Application Name=\"SQL Server Management Studio\";Command Timeout=0;Initial Catalog=CIS560");
            connection.Open();
        }

        public IEnumerable<Customer> GetAllCustomers()
        {
            List<Customer> data = new();
            using (SqlCommand command = new("SELECT CustomerID, CustomerName, AccountOpenedDate FROM Customers", connection))
            {
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        data.Add(new(reader.GetInt32(0), reader.GetString(1), reader.GetDateTime(2)));
                    }
                }
            }
            return data;
        }

        public IEnumerable<Top10Customer> GetTop10Customers()
        {
            List<Top10Customer> data = new();
            using SqlCommand command = new("EXECUTE GetTopCustomers '01-01-2000','12-31-2100';", connection);
            using SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                Top10Customer c = new()
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    AccountOpenedDate = reader.GetDateTime(2),
                    TotalOrders = reader.GetInt32(3),
                    CommonlyPurchasedProduct = reader.GetString(4),
                    QuantityPurchased = reader.GetInt32(5)
                };
                data.Add(c);
            }
            return data;
        }

        public IEnumerable<Product> GetAllProducts()
        {
            List<Product> data = new();
            string q = "SELECT P.ProductID, PT.ProductTypeName,P.ProductName, B.BrandName, P.QuantityOnHand, P.SellPrice, S.SupplierName,P.BulkPrice FROM Products P JOIN ProductTypes PT ON PT.ProductTypeID = P.ProductTypeID JOIN Brands B ON B.BrandID = P.BrandID JOIN Suppliers S ON S.SupplierID = P.SupplierID;";
            using (SqlCommand command = new(q, connection))
            {
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Product prod = new()
                        {
                            Id = reader.GetInt32(0),
                            Type = reader.GetString(1),
                            Name = reader.GetString(2),
                            BrandName = reader.GetString(3),
                            QuantityOnHand = reader.GetInt32(4),
                            SellPrice = reader.GetDecimal(5),
                            SupplierName = reader.GetString(6),
                            BulkPrice = reader.GetDecimal(7)
                        };
                        data.Add(prod);
                    }
                }
            }
            return data;
        }

        public IEnumerable<ProductSale> GetAllProductSales()
        {
            List<ProductSale> data = new();
            using SqlCommand command = new("EXECUTE GetProductSales '01-01-2000','12-31-2100';", connection);
            using SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                ProductSale prod = new()
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Year = reader.GetInt32(2),
                    Month = reader.GetInt32(3),
                    QuantitySold = reader.GetInt32(4),
                    TotalSales = reader.GetDecimal(5)
                };
                data.Add(prod);
            }
            return data;
        }

        public IEnumerable<SupplierProdSold> GetAllSupplierProdSold()
        {
            List<SupplierProdSold> data = new();
            using SqlCommand command = new("EXECUTE GetBestSellingProductsBySupplier '01-01-2000','12-31-2100';", connection);
            using SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                SupplierProdSold prod = new()
                {
                    SupplierId = reader.GetInt32(0),
                    SupplierName = reader.GetString(1),
                    TotalProductsSold = reader.GetInt32(2),
                    ProductName = reader.GetString(3),
                    ProductId = reader.GetInt32(4),
                    ProductPrice = reader.GetDecimal(5),
                    TotalProductSales = reader.GetDecimal(6)
                };
                data.Add(prod);
            }
            return data;
        }

        public IEnumerable<ProductTypeSales> GetMonthlyProductTypeSales()
        {
            List<ProductTypeSales> data = new();
            using SqlCommand command = new("EXECUTE GetMonthlyProductTypeSales '12-31-2026';", connection);
            using SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                ProductTypeSales prod = new()
                {
                    Type = reader.GetString(0),
                    TotalSales = reader.GetDecimal(1),
                    Month = reader.GetInt32(2),
                    Year = reader.GetInt32(3),
                    QuantitySold = reader.GetInt32(4),
                    MonthlyRank = reader.GetInt64(5)
                };
                data.Add(prod);
            }
            return data;
        }

        public void AddCustomer()
        {
            AddCustomerDialog dialog = new();
            if (!dialog.ShowDialog().GetValueOrDefault() || string.IsNullOrEmpty(dialog.CustomerName)) return;

            using SqlCommand command = new($"EXEC CreateCustomer '{dialog.CustomerName}';", connection);
            try
            {
                int i = command.ExecuteNonQuery();
                MessageBox.Show($"{i} row created");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }
        }

        public void AddProduct()
        {
            AddProductDialog dialog = new(this);
            if (dialog.ShowDialog().GetValueOrDefault())
            {
                using SqlCommand command = new($"EXEC CreateProduct '{dialog.ProductName}',{dialog.Quantity},{dialog.BulkPrice},{dialog.SellPrice},'{dialog.SupplierName}','{dialog.BrandName}','{dialog.ProductTypeName}';", connection);
                try
                {
                    int i = command.ExecuteNonQuery();
                    MessageBox.Show($"{i} row created");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        /// <summary>
        /// Gets and returns all supplier names, brand names, and product type names
        /// </summary>
        public void GetSuppliersBrandProductType(out List<string> suppliers, out List<string> brands, out List<string> productType)
        {
            suppliers = new();
            brands = new();
            productType = new();

            using SqlCommand command = new("SELECT SupplierName FROM Suppliers", connection);
            SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                suppliers.Add(reader.GetString(0));
            }
            reader.Close();

            command.CommandText = "SELECT BrandName FROM Brands";
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                brands.Add(reader.GetString(0));
            }
            reader.Close();

            command.CommandText = "SELECT ProductTypeName FROM ProductTypes";
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                productType.Add(reader.GetString(0));
            }
            reader.Close();
        }
    }
}
