using System.Collections;
using System.Collections.Generic;

namespace Redirxn.TeamKitty.Models
{
    public class Ledger
    {
        public IList<LedgerSummaryLine> Summary { get; set; } = new List<LedgerSummaryLine>();
        public TransactionList Transactions { get; set; } = new TransactionList();
    }
}