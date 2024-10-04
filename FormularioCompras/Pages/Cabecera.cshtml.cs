using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace FormularioCompras.Pages
{
    public class CabeceraModel : PageModel
    {
        [Required, MinLength(1, ErrorMessage = "Campo obligatorio")]
        [Display(Name = "Empresa")]
        public List<SelectListItem> empresa { get; set; }
        public List<SelectListItem> proveedor { get; set; }
        [BindProperty]
        public string username { get; set; }
        [BindProperty]
        public string code { get; set; }
        [BindProperty]
        public string codEmpresa { get; set; }
        [BindProperty]
        public string nombreEmpresa { get; set; }
        [BindProperty]
        public string codProveedor { get; set; }
        [BindProperty]
        public string nombreProveedor { get; set; }
        [BindProperty]
        [Required(ErrorMessage = "Campo obligatorio")]
        public DateTime fechaSolicitud { get; set; }
        [BindProperty]
        public Boolean esInexistente { get; set; }
        public Models.CabeceraModel cabeceraModel { get; set; }

        static string tenantId = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("AppSettings")["BcTenantId"];
        static string bcApi = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("AppSettings")["BcAPI"];
        static string companyId = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("AppSettings")["BcCompanyId"];
        static string BcEnvironment = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("AppSettings")["BcEnvironment"];

        public async Task<ActionResult> OnGet()
        {
            username = HttpContext.Session.GetString("usuario");
            code = HttpContext.Session.GetString("code");
            nombreEmpresa = HttpContext.Session.GetString("Company");
            //empresa = await CargarEmpresaAsync();
            proveedor = await CargarProveedorAsync();
            fechaSolicitud = DateTime.Now;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            cabeceraModel = new Models.CabeceraModel();
            string idRecord;
            if (ModelState.IsValid)
            {
                if (esInexistente)
                {
                    cabeceraModel = crearCabecera("", "", true, fechaSolicitud, username, nombreEmpresa);
                    idRecord = EnviarCabeceraAsync(cabeceraModel).Result;

                    if (idRecord != null)
                    {
                        HttpContext.Session.SetString("NameProveedor", nombreProveedor);
                        HttpContext.Session.SetString("CodProveedor", codProveedor);
                        HttpContext.Session.SetString("EsInexistente", "true");
                        HttpContext.Session.SetString("idRecord", idRecord);

                        return RedirectToPage("Lineas");
                    }
                }
                else
                {
                    string[] valor = nombreProveedor.Split('-');

                    cabeceraModel = crearCabecera(codProveedor, valor[1], false, fechaSolicitud, username, nombreEmpresa);
                    idRecord = EnviarCabeceraAsync(cabeceraModel).Result;

                    if(idRecord != null)
                    {
                        HttpContext.Session.SetString("NameProveedor", valor[1]);
                        HttpContext.Session.SetString("CodProveedor", codProveedor);
                        HttpContext.Session.SetString("EsInexistente", "false");
                        HttpContext.Session.SetString("idRecord", idRecord);

                        return RedirectToPage("Lineas");
                    }
                }
            }

            username = HttpContext.Session.GetString("usuario");
            code = HttpContext.Session.GetString("code");
            empresa = await CargarEmpresaAsync();
            proveedor = await CargarProveedorAsync();

            return Page();
        }

        public async Task<List<SelectListItem>> CargarProveedorAsync()
        {
            List<SelectListItem> lista = new List<SelectListItem>();

            lista.Add(new SelectListItem
            {
                Value = string.Empty,
                Text = "Seleccione una opción"
            });

            using (HttpClient client = new HttpClient())
            {
                string url = string.Format(bcApi + "/vendor",
                tenantId,
                BcEnvironment,
                companyId);

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
                            if (registro != null)
                            {
                                lista.Add(new SelectListItem
                                {
                                    Value = registro["no"].Value<string>(),
                                    Text = registro["no"].Value<string>() + " - " + registro["name"].Value<string>()
                                });
                            }
                        }
                    }
                }
                catch
                {
                }
            }

            return lista;
        }

        public async Task<List<SelectListItem>> CargarEmpresaAsync()
        {
            List<SelectListItem> lista = new List<SelectListItem>();

            lista.Add(new SelectListItem
            {
                Value = string.Empty,
                Text = "Seleccione una opción"
            });

            using (HttpClient client = new HttpClient())
            {
                string url = string.Format(bcApi + "/company1",
                tenantId,
                BcEnvironment,
                companyId);

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
                            if (registro != null)
                            {
                                lista.Add(new SelectListItem
                                {
                                    Value = registro["id"].Value<string>(),
                                    Text = registro["displayName"].Value<string>()
                                });
                            }
                        }
                    }
                }
                catch
                {
                }
            }

            return lista;
        }

        public Models.CabeceraModel crearCabecera(string codProveedor, string nombreProveedor, Boolean esInexistente, DateTime fechaSolicitud, string username, string nombreEmpresa)
        {
            Models.CabeceraModel cabecera = new Models.CabeceraModel();

            cabecera.vendorNo = codProveedor;
            cabecera.vendorName = nombreProveedor;
            cabecera.vendorNotExisting = esInexistente;
            cabecera.documentDate = fechaSolicitud.ToString("yyyy-MM-dd");
            cabecera.documentType = "RDA";
            cabecera.usuarioAutenticacion = username;
            cabecera.codusuarioAutenticacion = code;
            //cabecera.Company = nombreEmpresa;


            return cabecera;
        }

        public async Task<string> EnviarCabeceraAsync(Models.CabeceraModel cabeceraModel)
        {
            using (HttpClient client = new HttpClient())
            {
                string url = string.Format(bcApi + "/solicitudCabecera",
                tenantId,
                BcEnvironment,
                companyId);

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
                        string json = JsonConvert.SerializeObject(cabeceraModel);
                        StringContent httpContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                        response = await client.PostAsync(url, httpContent);
                        if (response.IsSuccessStatusCode)
                        {
                            //Obtener el id de la respuesta
                            var resultado = await response.Content.ReadAsStringAsync();
                            var jsonResult = JObject.Parse(resultado);
                            var idRecord = ((JValue)jsonResult["idRecord"]).Value<string>();
                            if (!string.IsNullOrEmpty(idRecord))
                                return idRecord;
                            else
                                return null;
                        }
                    }
                }
                catch
                {
                    return null;
                }
            }
            return null;
        }
    }
}
