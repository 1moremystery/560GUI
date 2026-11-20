using _560GUI.DataClasses;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public async Task<object[]> TestQuery()
        {
            object[] data = new object[2];
            string q = "SELECT * FROM Clubs.Club;";
            using (SqlCommand command = new(q, connection))
            {
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    int row = 0;
                    while (reader.Read())
                    {
                        TestObject test = new();
                        test.ClubId = reader.GetValue(0);
                        test.Name = reader.GetValue(1);
                        test.Purpose = reader.GetValue(2);
                        test.CreatedOn = reader.GetValue(3);
                        test.UpdatedOn = reader.GetValue(4);
                        data[row] = test;
                        row++;
                    }
                }
            }
            return data;
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
    }
}
