using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _560GUI.DataClasses
{
    public class Product
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public string BrandName { get; set; }
        public int QuantityOnHand { get; set; }
        public decimal SellPrice { get; set; }
        public string SupplierName { get; set; }
        public decimal BulkPrice { get; set; }
    }
}
