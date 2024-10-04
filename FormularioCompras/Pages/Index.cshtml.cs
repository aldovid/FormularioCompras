using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace FormularioCompras.Pages
{
    public class IndexModel : PageModel
    {
        [BindProperty]
        [Required(ErrorMessage = "Campo obligatorio")]
        public string username { get; set; }
        [BindProperty]
        [Required(ErrorMessage = "Campo obligatorio")]
        public string password { get; set; }
        public string msg;

        static string tenantId = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("AppSettings")["BcTenantId"];
        static string bcApi = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("AppSettings")["BcAPI"];
        static string companyId = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("AppSettings")["BcCompanyId"];
        static string BcEnvironment = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("AppSettings")["BcEnvironment"];

        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }


        public async Task<IActionResult> OnPostAsync()
        {
            if(ModelState.IsValid)
            {
                using (HttpClient client = new HttpClient())
                {
                    string filter = "?$filter=userName eq '" + username + "'";
                    string url = string.Format(bcApi + "/userPurchaseOrden",
                    tenantId,
                    BcEnvironment,
                    companyId,
                    filter);

                    try
                    {
                        string token = await Conexion.ConfigBusinessCentral.GetToken();

                        client.BaseAddress = new Uri(url);
                        client.Timeout = new TimeSpan(0, 10, 0);
                        client.MaxResponseContentBufferSize = int.MaxValue;
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(
                            new MediaTypeWithQualityHeaderValue("application/json"));
                        client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

                        var response = await client.GetAsync("");
                        if (response.IsSuccessStatusCode)
                        {
                            var resultado = await response.Content.ReadAsStringAsync();
                            var jsonResult = JObject.Parse(resultado);
                            var registros = (JArray)jsonResult["value"];
                            foreach (var registro in registros)
                            {
                                if (registro["description"].Value<string>().Equals(username) && registro["password"].Value<string>().Equals(password))
                                {
                                    HttpContext.Session.SetString("usuario", registro["description"].Value<string>());
                                    HttpContext.Session.SetString("code", registro["code"].Value<string>());
                                    HttpContext.Session.SetString("Company", registro["Company"].Value<string>());
                                    return RedirectToPage("Cabecera");
                                }
                                else
                                {
                                    msg = "Usuario o contraseña incorrecta.";
                                }
                            }
                        }
                        else
                        {
                            msg = "No se pudo completar la operacion." + System.Environment.NewLine + "Vuelva a intentar";
                        }

                    }
                    catch
                    {
                        msg = "No se pudo completar la operacion." + System.Environment.NewLine + "Vuelva a intentar";

                    }
                }
            
            }

            return Page();

        }
    }
}
