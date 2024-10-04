using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace FormularioCompras.Conexion
{
    public class ConfigBusinessCentral
    {
        static string tenantId = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("AppSettings")["BcTenantId"];
        static string bcApi = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("AppSettings")["BcAPI"];
        static string companyId = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("AppSettings")["BcCompanyId"];
        static string BcEnvironment = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("AppSettings")["BcEnvironment"];

        static string clientId = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("AppSettings")["BCClientId"];
        static string secret = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("AppSettings")["BcSecretId"];
        static string scope = "https://api.businesscentral.dynamics.com/.default";

        public static async Task<string> GetToken()
        {
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            string token = string.Empty;

            try
            {
                string URL = string.Format("https://login.microsoftonline.com/{0}/oauth2/v2.0/token", tenantId);

                HttpClient client = new HttpClient();
                client.Timeout = TimeSpan.FromMilliseconds(2000);
                var content = new StringContent("grant_type=client_credentials" +
                                                "&client_id=" + clientId +
                                                "&client_secret=" + secret +
                                                "&scope=" + scope);

                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/x-www-form-urlencoded");

                var response = await client.PostAsync(URL, content);
                if (response.IsSuccessStatusCode)
                {
                    JObject Result = JObject.Parse(await response.Content.ReadAsStringAsync());
                    token = Result["access_token"].ToString();
                }
            }
            catch (Exception ex) { }

            return token;
        }
    }
}
