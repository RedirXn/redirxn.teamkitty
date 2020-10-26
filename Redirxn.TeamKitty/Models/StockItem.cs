using System;
using System.Collections.Generic;
using System.Text;

namespace Redirxn.TeamKitty.Models
{
    public class StockItem
    {
        public string MainName { get; set; }
        public string PluralName { get; set; }
        public string StockGrouping { get; set; }
        public decimal SalePrice { get; set; }
        public decimal StockPrice { get; set; }
    }    
}
