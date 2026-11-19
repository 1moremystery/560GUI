using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;


namespace _560GUI
{
    public class MainController
    {
        
        SqlConnection connection; 

        public MainController()
        {
            connection = new("Data Source=(localdb)\\MSSQLLocalDb;Integrated Security=True;Persist Security Info=False;Pooling=False;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Application Name=\"SQL Server Management Studio\";Command Timeout=0;Initial Catalog=CIS560");
        }

        public async Task<object[,]> TestQuery()
        {
            object[,] data = new object[10,10];
            string q = "SELECT * FROM Clubs.Club;";
            using( SqlCommand command = new(q,connection))
            {
                connection.Open();
                using(SqlDataReader reader = command.ExecuteReader())
                {
                    int row = 0;
                    while(reader.Read())
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            data[row,i] = reader.GetValue(i);
                        }
                        row++;
                    }
                }
            }
            return data;
        }
    }
}
