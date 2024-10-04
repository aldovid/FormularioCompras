using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FormularioCompras.Models
{
    public class CabeceraModel
    {
        public string documentType { get; set; }
        public string vendorName { get; set; }
        public string vendorNo { get; set; }
        public Boolean vendorNotExisting { get; set; }
        public string documentDate { get; set; }
        public string usuarioAutenticacion { get; set; }
        public string codusuarioAutenticacion { get; set; }
       // public string Company { get; set; }
    }
}
