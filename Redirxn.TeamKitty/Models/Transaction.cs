using System;

namespace Redirxn.TeamKitty.Models
{
    public class Transaction
    {
        public Member Person { get; set; } = new Member();
        public DateTime Date { get; set; }
        public TransactionType TransactionType { get; set; }
        public string TransactionName { get; set; }
        public int TransactionCount { get; set; }
        public decimal TransactionAmount { get; set; }
    }
}