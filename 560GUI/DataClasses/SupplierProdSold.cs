using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _560GUI.DataClasses
{
    public class SupplierProdSold
    {
        public int SupplierId { get; set; }
        public string SupplierName { get; set; }
        public int TotalProductsSold { get; set; }
        public string ProductName { get; set; }
        public int ProductId { get; set; }
        public decimal ProductPrice { get; set; }
        public decimal TotalProductSales { get; set; }
    }
}
