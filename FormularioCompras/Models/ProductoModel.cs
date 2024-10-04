using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FormularioCompras.Models
{
    public class ProductoModel
    {
        public string no { get; set; }
        public string description { get; set; }
        public string purchUnitOfMeasure { get; set; }
        public string unitPrice { get; set; }
    }
}
