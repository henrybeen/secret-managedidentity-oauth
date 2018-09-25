using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;

namespace Demo.KeyVaultManagedIdentity.Application.Controllers
{
    public class HomeController : Controller
    {
        public async Task<ActionResult> Index()
        {
            var isManagedIdentity = Environment.GetEnvironmentVariable("MSI_ENDPOINT") != null
                                    && Environment.GetEnvironmentVariable("MSI_SECRET") != null;

            ViewBag.IsManagedIdentity = isManagedIdentity;

            if (isManagedIdentity)
            {
                var azureServiceTokenProvider = new AzureServiceTokenProvider();
                var accessToken = await azureServiceTokenProvider.GetAccessTokenAsync("https://database.windows.net/");

                var builder = new SqlConnectionStringBuilder
                {
                    DataSource = ConfigurationManager.AppSettings["databaseServerName"] + ".database.windows.net",
                    InitialCatalog = ConfigurationManager.AppSettings["databaseName"],
                    ConnectTimeout = 30
                };

                if (accessToken == null)
                {
                    ViewBag.Secret = "Failed to acuire the token to the database.";
                }
                else
                {
                    using (var connection = new SqlConnection(builder.ConnectionString))
                    {
                        connection.AccessToken = accessToken;
                        connection.Open();

                        ViewBag.Secret = "Connected to the database!";
                    }
                }
            }

            return View();
        }
    }
}