using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _560GUI.DataClasses
{
    public class Top10Customer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime AccountOpenedDate { get; set; }
        public int TotalOrders { get; set; }
        public string CommonlyPurchasedProduct { get; set; }
        public int QuantityPurchased {  get; set; }
    }
}
