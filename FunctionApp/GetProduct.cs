using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Data.SqlClient;
using System.Collections.Generic;

namespace FunctionApp
{
    public static class GetProduct
    {
        [FunctionName("GetProducts")]
        public static async Task<IActionResult> RunProducts(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req)
        {
            SqlConnection conn = GetConnection();
            List<Product> products = new List<Product>();
            string statement = "Select ProductID, ProductName, Quantity from Products";

            conn.Open();
            SqlCommand cmd = new SqlCommand(statement, conn);
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    Product product = new Product()
                    {
                        ProductID = reader.GetInt32(0),
                        ProductName = reader.GetString(1),
                        Quantity = reader.GetInt32(2)
                    };

                    products.Add(product);
                }
            }
            conn.Close();

            return new OkObjectResult(products);
        }

        [FunctionName("GetProduct")]
        public static async Task<IActionResult> RunProduct(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req)
        {
            int productId = int.Parse(req.Query["id"]);
            Product product = new();

            SqlConnection conn = GetConnection();
            string statement = string.Format("Select ProductID, ProductName, Quantity from Products where ProductID={0}", productId);

            conn.Open();
            SqlCommand cmd = new SqlCommand(statement, conn);

            try
            {
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    reader.Read();
                    product.ProductID = reader.GetInt32(0);
                    product.ProductName = reader.GetString(1);
                    product.Quantity = reader.GetInt32(2);

                    return new OkObjectResult(product);
                }
            }
            catch (System.Exception)
            {
                return new OkObjectResult("No records found.");
            }
            finally
            {
                conn.Close();
            }
        }


        private static SqlConnection GetConnection()
        {
            string connectionString = "Server=tcp:appserver-dev-001.database.windows.net,1433;Initial Catalog=sql-db-ene-dev-001;Persist Security Info=False;User ID=sqladmin;Password=Admin123*;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
            
            return new SqlConnection(connectionString);
        }
    }
}
