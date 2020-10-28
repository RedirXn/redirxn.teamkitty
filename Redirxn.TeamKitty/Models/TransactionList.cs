using System;

namespace Redirxn.TeamKitty.Models
{
    public class TransactionList
    {
        public Member Person { get; set; } = new Member();
        public DateTime Date { get; set; }
        public TransactionType Transaction { get; set; }
        public string TransactionName { get; set; }
        public int TransactionCount { get; set; }
        public decimal TransactionAmount { get; set; }
    }
}