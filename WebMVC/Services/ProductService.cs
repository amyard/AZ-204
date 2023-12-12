using System.Data.SqlClient;
using WebMVC.Models;

namespace WebMVC.Services
{
    public class ProductService : IProductService
    {
        //private static string db_source = "appserver-dev-001.database.windows.net";
        //private static string db_user = "sqladmin";
        //private static string db_password = "Admin123*";
        //private static string db_database = "sql-db-ene-dev-001";

        private readonly IConfiguration _configuration;

        public ProductService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private SqlConnection GetConnection()
        {
            //var _builder = new SqlConnectionStringBuilder();
            //_builder.DataSource = db_source;
            //_builder.UserID = db_user;
            //_builder.Password = db_password;
            //_builder.InitialCatalog = db_database;

            // we are using it when getting Connection string from resource "Web App" ---> Configurations.
            //return new SqlConnection(_configuration.GetConnectionString("SQLConnection"));

            // we are using it when get connection string from resource "App Configuration"
            return new SqlConnection(_configuration["SQLConnection"]);
        }

        public List<Product> GetProducts()
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

            return products;
        }

    }
}
