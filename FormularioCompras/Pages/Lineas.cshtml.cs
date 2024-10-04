using FormularioCompras.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace FormularioCompras.Pages
{
    public class LineasModel : PageModel
    {
        public DateTime fechaSolicitud;
        public string msg;

        [BindProperty]
        public string empresa { get; set; }
        [BindProperty]
        public Boolean esInexistente { get; set; }
        [BindProperty]
        public string codProveedor { get; set; }
        [BindProperty]
        public string nombreProveedor { get; set; }
        [BindProperty]
        public string codEmpresa { get; set; }
        [BindProperty]
        public string idRecord { get; set; }
        public LineaModel LineaModel { get; private set; }
        [BindProperty]
        public List<SelectListItem> tipo { get; set; }
        [BindProperty]
        public List<SelectListItem> producto { get; set; }
        [BindProperty]
        public List<Models.LineaModel> lineas { get; set; }
        [BindProperty]
        public DateTime fechaRequerida { get; set; }
        [BindProperty]
        public string codTipo { get; set; }
        [BindProperty]
        public string nombreTipo { get; set; }
        [BindProperty]
        public string codProducto { get; set; }
        [BindProperty]
        public string nombreProducto { get; set; }
        [BindProperty]
        public string unidadMedida { get; set; }
        [BindProperty]
        public string precio { get; set; }
        [BindProperty]
        public string cantidad { get; set; }
        [BindProperty]
        public string porDescuento { get; set; }
        [BindProperty]
        public string descuento { get; set; }
        [BindProperty]
        public string importe { get; set; }
        [BindProperty]
        public int nroLinea { get; set; }
        [BindProperty]
        public decimal total { get; set; }

        static string tenantId = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("AppSettings")["BcTenantId"];
        static string bcApi = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("AppSettings")["BcAPI"];
        static string companyId = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("AppSettings")["BcCompanyId"];
        static string BcEnvironment = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("AppSettings")["BcEnvironment"];

        public async Task OnGetAsync()
        {
            empresa = HttpContext.Session.GetString("NameEmpresa");
            fechaRequerida = DateTime.Now;
            nroLinea = 0;
            precio = "0";
            cantidad = "0";
            descuento = "0";
            porDescuento = "0";
            empresa = HttpContext.Session.GetString("NameEmpresa");
            codProveedor = HttpContext.Session.GetString("CodProveedor");
            nombreProveedor = HttpContext.Session.GetString("NameProveedor");
            esInexistente = HttpContext.Session.GetString("EsInexistente") == "true" ? true : false;
            idRecord = HttpContext.Session.GetString("idRecord");

            tipo = CargarTipo();
            producto = await CargarProductoAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            total = 0;
            msg = "";
            string[] valor = nombreProducto.Split("-");
            if (ModelState.IsValid)
            {
                if(esInexistente)
                {
                    LineaModel = crearLinea(true, fechaRequerida, nombreTipo, idRecord,
                        "", "", valor[1], codProducto, unidadMedida, precio, cantidad, porDescuento,
                        descuento, importe);
                    //idRecord = EnviarLineaAsync(LineaModel).Result;

                    if (idRecord != null)
                    {
                        msg = "Datos cargados correctamente";
                    }
                }
                else
                {
                    LineaModel = crearLinea(false, fechaRequerida, nombreTipo, idRecord,
                        nombreProveedor, codProveedor, valor[1], codProducto, unidadMedida, precio, cantidad, porDescuento,
                        descuento, importe);
                    idRecord = EnviarLineaAsync(LineaModel).Result;

                    if (idRecord != null)
                    {
                        msg = "Linea Agregado correctamente";
                    }
                }
            }

            empresa = HttpContext.Session.GetString("NameEmpresa");
            fechaRequerida = DateTime.Now;
            nroLinea = 0;
            precio = "0";
            cantidad = "0";
            descuento = "0";
            porDescuento = "0";
            empresa = HttpContext.Session.GetString("NameEmpresa");
            codProveedor = HttpContext.Session.GetString("CodProveedor");
            nombreProveedor = HttpContext.Session.GetString("NameProveedor");
            esInexistente = HttpContext.Session.GetString("EsInexistente") == "true" ? true : false;
            idRecord = HttpContext.Session.GetString("idRecord");
            lineas = cargaLineasAsync(idRecord).Result;

            if(lineas.Count > 0)
            {
                foreach(var item in lineas)
                {
                    total += item.lineAmount;
                }
            }

            tipo = CargarTipo();
            producto = await CargarProductoAsync();

            return Page();
        }

        public Models.LineaModel crearLinea(Boolean esInexistente, DateTime fechaRequerida, string nombreTipo,
            string idRecord, string nombreProv, string codProv, string nombreProducto, string codProducto,
            string unidadMedida, string precio, string cantidad, string porDescuento, string descuento,
            string importe)
        {
            Models.LineaModel Linea = new Models.LineaModel();

            Linea.lineNo = 1000;
            Linea.type = nombreTipo == "Producto" ? "Item" : "Item";
            Linea.no = codProducto;
            Linea.idRecord = int.Parse(idRecord);
            Linea.description = nombreProducto;
            Linea.lineAmount = decimal.Parse(importe);
            Linea.lineDiscount = decimal.Parse(porDescuento);
            Linea.lineDiscountAmount = decimal.Parse(descuento);
            Linea.quantity = decimal.Parse(cantidad);
            Linea.unitOfMeasure = unidadMedida;
            Linea.unitPrice = decimal.Parse(precio);
            Linea.vendorNo = codProv;
            Linea.vendorName = nombreProv;
            Linea.vendorNotExisting = esInexistente;
            Linea.competenceDate = fechaRequerida.ToString("yyyy-MM-dd");
            Linea.documentType = "RDA";

            return Linea;
        }

        public async Task<string> EnviarLineaAsync(Models.LineaModel lineaModel)
        {
            using (HttpClient client = new HttpClient())
            {
                string url = string.Format(bcApi + "/solicitudLinea",
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
                        string json = JsonConvert.SerializeObject(lineaModel);
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

        public List<SelectListItem> CargarTipo()
        {
            List<SelectListItem> lista =  new List<SelectListItem>();
            lista.Add(new SelectListItem
            {
                Value = "",
                Text = "Seleccione el tipo"
            });
            lista.Add(new SelectListItem
            {
                Value = "001",
                Text = "Producto"
            });

            return lista;
        }

        public async Task<List<SelectListItem>> CargarProductoAsync()
        {
            List<SelectListItem> lista = new List<SelectListItem>();

            lista.Add(new SelectListItem
            {
                Value = string.Empty,
                Text = "Seleccione una opción"
            });

            using (HttpClient client = new HttpClient())
            {
                string url = string.Format(bcApi + "/item",
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
                                    Text = registro["no"].Value<string>() + " - " + registro["description"].Value<string>(),
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

        public async Task<List<LineaModel>> cargaLineasAsync(string idRecord)
        {
            List<Models.LineaModel> lista = new List<Models.LineaModel>();

            using (HttpClient client = new HttpClient())
            {
                string filter = "?$filter=idRecord eq " + idRecord;
                string url = string.Format(bcApi + "/solicitudLinea",
                tenantId,
                BcEnvironment,
                companyId);

                url = url + filter;

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
                                lista.Add(new LineaModel()
                                {
                                    description = registro["description"].Value<string>(),
                                    quantity = registro["quantity"].Value<decimal>(),
                                    unitPrice = registro["unitPrice"].Value<decimal>(),
                                    lineDiscountAmount = registro["lineDiscountAmount"].Value<decimal>(),
                                    lineAmount = registro["lineAmount"].Value<decimal>(),
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

        [HttpPost]
        public async Task<IActionResult> BuscarProducto(string selectedValue)
        {
            using (HttpClient client = new HttpClient())
            {
                Models.LineaModel linea = new Models.LineaModel();
                string filter = "?$filter=no eq " + selectedValue;
                string url = string.Format(bcApi + "/item",
                tenantId,
                BcEnvironment,
                companyId);

                url = url + filter;

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
                                unidadMedida = registro["purchUnitOfMeasure"].Value<string>();
                                precio = registro["unitCost"].Value<string>();
                                porDescuento = registro["unitPrice"].Value<string>();
                            }
                        }
                    }
                }
                catch
                {
                }
            }

            return Page();
        }
    }
}
