using Redirxn.TeamKitty.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Redirxn.TeamKitty.ViewModels
{
    public class WipTransaction
    {
        public Dictionary<string, int> Purchases = new Dictionary<string, int>();
        public Dictionary<string, int> Provisions = new Dictionary<string, int>();
        public Dictionary<string, decimal> Carries = new Dictionary<string, decimal>();
        public decimal Adjustments = 0M;
        public decimal Payments = 0M;
        public decimal Total = 0M;
        public string Date = string.Empty;
        public string Person;

        public void UpdateFromTransaction(string date, Transaction item)
        {
            Person = item.Person.DisplayName;
            switch (item.TransactionType)
            {
                case TransactionType.CarryOver:
                    {
                        if (Carries.ContainsKey(item.TransactionName))
                        {
                            Carries[item.TransactionName] += item.TransactionAmount;
                        }
                        else
                        {
                            Carries[item.TransactionName] = item.TransactionAmount;
                        }
                        Date = date;
                        Payments += item.TransactionAmount;
                        Total += item.TransactionAmount;
                        break;
                    }
                case TransactionType.Payment:
                    {
                        Date = date;
                        Payments += item.TransactionAmount;
                        Total += item.TransactionAmount;
                        break;
                    }
                case TransactionType.Purchase:
                    {
                        if (Purchases.ContainsKey(item.TransactionName))
                        {
                            Purchases[item.TransactionName] += item.TransactionCount;
                        }
                        else
                        {
                            Purchases[item.TransactionName] = item.TransactionCount;
                        }
                        Date = date;
                        Total -= item.TransactionAmount;
                        break;
                    }
                case TransactionType.Provision:
                    {
                        if (Provisions.ContainsKey(item.TransactionName))
                        {
                            Provisions[item.TransactionName] += item.TransactionCount;
                        }
                        else
                        {
                            Provisions[item.TransactionName] = item.TransactionCount;
                        }
                        Date = date;
                        break;
                    }
                case TransactionType.Adjustment:
                    {
                        Date = date;
                        Adjustments += item.TransactionAmount;
                        Total += item.TransactionAmount;
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }

        public GroupedTransaction ToGroupedTran()
        {
            var payments = (Payments > 0M) ? "Paid: " + string.Format("{0:C}", Payments) : string.Empty;
            var purchases = (Purchases.Count > 0) ? "Purchased: " + string.Join(", ", Purchases.Select(kv => kv.Value + " " + kv.Key).ToArray()) : string.Empty;
            var provisions = (Provisions.Count > 0) ? "Supplied: " + string.Join(", ", Provisions.Select(kv => kv.Value + " " + kv.Key).ToArray()) : string.Empty;
            var carried = (Carries.Count > 0) ? "Carried Over: " + string.Join(", ", Carries.Select(kv => string.Format("{0:C}", kv.Value) + " " + kv.Key).ToArray()) : string.Empty;
            var adjustments = (Adjustments != 0M) ? "Adjusted: " + string.Format("{0:C}", Adjustments) : string.Empty;

            string summary = purchases + ((!string.IsNullOrEmpty(purchases) && !string.IsNullOrEmpty(provisions)) ? Environment.NewLine : string.Empty) +
                provisions + ((!string.IsNullOrEmpty(purchases + provisions) && !string.IsNullOrEmpty(payments)) ? Environment.NewLine : string.Empty) +
                payments + ((!string.IsNullOrEmpty(purchases + provisions + payments) && !string.IsNullOrEmpty(carried)) ? Environment.NewLine : string.Empty) +
                carried + ((!string.IsNullOrEmpty(purchases + provisions + payments + carried) && !string.IsNullOrEmpty(adjustments)) ? Environment.NewLine : string.Empty) +
                adjustments;

            return new GroupedTransaction()
            {
                Date = Date,
                Person = Person,
                DayTotal = (Total != 0M) ? string.Format("{0:C}", Total) : string.Empty,
                Summary = summary
            };
        }


    }
}
