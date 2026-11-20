using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _560GUI.DataClasses
{
    public class ProductTypeSales
    {
        public string Type { get; set; }
        public decimal TotalSales { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public int QuantitySold { get; set; }
        public long MonthlyRank { get; set; }
    }
}
