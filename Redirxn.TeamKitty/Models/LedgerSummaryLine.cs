using System.Collections.Generic;

namespace Redirxn.TeamKitty.Models
{
    public class LedgerSummaryLine
    {
        public Member Person { get; set; } = new Member();
        public IList<Purchase> Purchases { get; set; } = new List<Purchase>();
        public IDictionary<string, int> Provisions { get; set; } = new Dictionary<string, int>();
        public decimal TotalOwed { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal Balance { get; set; }
    }
}