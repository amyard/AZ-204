using Microsoft.FeatureManagement;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System.Data.SqlClient;
// using System.Data.SqlClient;
using System.Text.Json;
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
        private readonly IFeatureManager _featureManager;

        public ProductService(IConfiguration configuration, IFeatureManager featureManager)
        {
            _configuration = configuration;
            _featureManager = featureManager;
        }

        public async Task<bool> IsBeta()
        {
            return await _featureManager.IsEnabledAsync("beta");
        }

        //private SqlConnection GetConnection()
        //{
        //    //var _builder = new SqlConnectionStringBuilder();
        //    //_builder.DataSource = db_source;
        //    //_builder.UserID = db_user;
        //    //_builder.Password = db_password;
        //    //_builder.InitialCatalog = db_database;

        //    // we are using it when getting Connection string from resource "Web App" ---> Configurations.
        //    //return new SqlConnection(_configuration.GetConnectionString("SQLConnection"));

        //    // we are using it when get connection string from resource "App Configuration"
        //    return new SqlConnection(_configuration["SQLConnection"]);
        //}

        private MySqlConnection GetConnection()
        {
            //var _builder = new SqlConnectionStringBuilder();
            //_builder.DataSource = db_source;
            //_builder.UserID = db_user;
            //_builder.Password = db_password;
            //_builder.InitialCatalog = db_database;

            // we are using it when getting Connection string from resource "Web App" ---> Configurations.
            //return new SqlConnection(_configuration.GetConnectionString("SQLConnection"));

            // we are using it when get connection string from resource "App Configuration"
            // return new MySqlConnection(_configuration["SQLConnection"]);

            var builder = new MySqlConnectionStringBuilder
            {
                // if we are using MySQL as entire Application
                Server = "mysql-server-name-2.mysql.database.azure.com",
                UserID = "sqladmin",
                Password = "Admin123*",
                Database = "appdb",
                SslMode = MySqlSslMode.VerifyCA,
                SslCa = "DigiCertGlobalRootG2.crt.pem",

                // if we are using MySQL from docker instance
                //Server = "20.93.46.107",
                //UserID = "root",
                //Password = "Admin123*",
                //Database = "appdb",
                //SslMode = MySqlSslMode.None,
                // SslCa = "DigiCertGlobalRootG2.crt.pem",
            };

            return new MySqlConnection(builder.ConnectionString);
        }

        // get data from DB
        public async Task<List<Product>> GetProducts()
        {
            MySqlConnection conn = GetConnection();
            List<Product> products = new List<Product>();
            string statement = "Select ProductID, ProductName, Quantity from Products";

            conn.Open();
            MySqlCommand cmd = new MySqlCommand(statement, conn);
            using (MySqlDataReader reader = cmd.ExecuteReader())
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

            return await _featureManager.IsEnabledAsync("beta")
                ? products.Take(2).ToList()
                : products;
        }

        // get data with Azure Functions
        //public async Task<List<Product>> GetProducts()
        //{
        //    string fucntionURL = "https://func-01-ene-dev-001.azurewebsites.net/api/GetProducts?code=CUNQ4OX2kGOUu0djOOpHHUenUomBimLTZHNU76rQHDh-AzFuU8lK8A==";

        //    using (HttpClient client = new HttpClient())
        //    {
        //        HttpResponseMessage response = await client.GetAsync(fucntionURL);
        //        string content = await response.Content.ReadAsStringAsync();
        //        List<Product> products = JsonConvert.DeserializeObject<List<Product>>(content);
                
        //        return await _featureManager.IsEnabledAsync("beta")
        //            ? products.Take(2).ToList()
        //            : products;
        //    }
        //}
    }
}
