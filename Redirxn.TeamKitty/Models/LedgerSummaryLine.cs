using System.Collections.Generic;

namespace Redirxn.TeamKitty.Models
{
    public class LedgerSummaryLine
    {
        public Member Person { get; set; } = new Member();
        public IList<Purchase> Purchases { get; set; } = new List<Purchase>();
        public IList<Payment> Payments { get; set; } = new List<Payment>();
        public IList<Provision> Provisions { get; set; } = new List<Provision>();
        public decimal TotalOwed { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal Balance { get; set; }
    }
}