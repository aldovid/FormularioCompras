using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FormularioCompras.Models
{
    public class LineaModel
    {
        public int idRecord { get; set; }
        public int lineNo { get; set; }
        public string vendorNo { get; set; }
        public Boolean vendorNotExisting { get; set; }
        public string competenceDate { get; set; }
        public string vendorName { get; set; }
        public string documentType { get; set; }
        public string type { get; set; }
        public string no { get; set; }
        public string description { get; set; }
        public string unitOfMeasure { get; set; }
        public decimal unitPrice { get; set; }
        public decimal quantity { get; set; }
        public decimal lineDiscount { get; set; }
        public decimal lineDiscountAmount { get; set; }
        public decimal lineAmount { get; set; }
    }
}
