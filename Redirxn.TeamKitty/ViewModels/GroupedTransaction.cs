using System.Collections.Generic;

namespace Redirxn.TeamKitty.ViewModels
{
    public class GroupedTransaction
    {
        public string Date { get; set; }        
        public string Person { get; set; }
        public string Summary { get; set; }
        public string DayTotal { get; set; }
    }

    public class DatedGroupedTransaction : List<GroupedTransaction>
    {
        public string Name { get; set; }
        public DatedGroupedTransaction(string name, List<GroupedTransaction> groupedTransactions) : base(groupedTransactions)
        {
            Name = name;
        }        
    }
}
