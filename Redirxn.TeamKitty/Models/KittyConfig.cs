using System;
using System.Collections.Generic;
using System.Text;

namespace Redirxn.TeamKitty.Models
{
    public class KittyConfig
    {
        public IEnumerable<StockItem> StockItems { get; set; } = new StockItem[0];
        

    }
}
