using Redirxn.TeamKitty.Models;
using Redirxn.TeamKitty.Services.Gateway;
using Splat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Redirxn.TeamKitty.Services.Logic
{
    public class KittyService : IKittyService
    {
        public Kitty Kitty { get; private set; }

        IKittyDataStore _dataStore;

        public KittyService(IKittyDataStore dataStore = null)
        {
            _dataStore = dataStore ?? Locator.Current.GetService<IKittyDataStore>();
        }

        public async Task LoadKitty(string kittyId)
        {
            Kitty = await _dataStore.GetKitty(kittyId ?? string.Empty);
            if (Kitty == null)
            {
                Kitty = new Kitty();
            }                        
        }

        public async Task SaveStockItem(StockItem stockItem)
        {
            var kitty = await _dataStore.GetKitty(Kitty.Id);

            if (kitty.KittyConfig.StockItems.Any(si => si.MainName == stockItem.MainName))
            {
                kitty.KittyConfig.StockItems = ReplaceStockItem(kitty.KittyConfig.StockItems, stockItem);
            }
            else
            {
                kitty.KittyConfig.StockItems = kitty.KittyConfig.StockItems.Concat(new[] { stockItem });
            }

            await _dataStore.SaveKittyToDb(kitty);
            Kitty = kitty;
        }

        public async Task DeleteStockItem(string mainName)
        {
            var kitty = await _dataStore.GetKitty(Kitty.Id);

            if (!kitty.KittyConfig.StockItems.Any(si => si.MainName == mainName))
            {
                return;
            }

            kitty.KittyConfig.StockItems = RemoveStockItem(kitty.KittyConfig.StockItems, mainName);

            await _dataStore.SaveKittyToDb(kitty);
            Kitty = kitty;
        }

        public async Task<string> CreateNewKitty(string email, string userName, string newKittyName)
        {
            var kitty = new Kitty
            {
                Id = email + '|' + newKittyName,
                Administrators = new [] { email },
            };

            kitty.Ledger.Summary.Add(new LedgerSummaryLine
            {
                Person = new Member { DisplayName = userName, Email = email },
                Balance = 0M,
                TotalOwed = 0M,
                TotalPaid = 0M
            });

            await _dataStore.SaveKittyToDb(kitty);
            Kitty = kitty;
            return Kitty.Id;
        }
        public async Task AddNewUser(string name)
        {
            await AddUser(name);
        }
        public async Task AddRegisteredUser(string email, string name, string kittyId)
        {
            await AddUser(name, email, kittyId);
        }
        public async Task AddUser(string name, string email = null, string kittyId = null)
        {
            var kitty = await _dataStore.GetKitty(kittyId ?? Kitty.Id);

            if ((email == null && kitty.Ledger.Summary.Any(m => m.Person.DisplayName == name))
                || (kitty.Ledger.Summary.Any(m => m.Person.Email == email)))
            {
                return;
            }
            
            kitty.Ledger.Summary.Add(new LedgerSummaryLine
            {
                Person = new Member { DisplayName = name, Email = email ?? name },
                Balance = 0M,
                TotalOwed = 0M,
                TotalPaid = 0M
            });

            await _dataStore.SaveKittyToDb(kitty);
            Kitty = kitty;
        }
        public bool AmIAdmin(string email)
        {
            return Kitty.Administrators.Any(s => s.Equals(email, StringComparison.OrdinalIgnoreCase));
        }

        public async Task TickMeASingle(string email, string personDisplayName, StockItem item)
        {
            var kitty = await _dataStore.GetKitty(Kitty.Id);

            var me = new Member
            {
                Email = email,
                DisplayName = personDisplayName
            };

            kitty.Ledger.Transactions.Add(new Transaction
            {
                Date = DateTime.Now,
                Person = me,
                TransactionType = TransactionType.Purchase,
                TransactionAmount = item.SalePrice,
                TransactionCount = 1,
                TransactionName = item.MainName
            });

            kitty = RecalculateLedgerSummary(kitty, email);
            await _dataStore.SaveKittyToDb(kitty);
            Kitty = kitty;

        }

        private Kitty RecalculateLedgerSummary(Kitty kitty, string email)
        {
            var lsl = kitty.Ledger.Summary.FirstOrDefault(s => s.Person.Email == email);
            lsl.Purchases = new List<Purchase>();
            foreach(var si in kitty.KittyConfig.StockItems)
            {
                var filtered = kitty.Ledger.Transactions.Where(t =>
                        t.Person.Email == lsl.Person.Email &&
                        t.TransactionType == TransactionType.Purchase &&
                        t.TransactionName == si.MainName
                        );
                lsl.Purchases.Add(new Purchase
                {
                    ProductName = si.MainName,
                    ProductCount = filtered.Sum(t => t.TransactionCount),
                    ProductTotal = filtered.Sum(t => t.TransactionAmount)
                });
            }
            lsl.TotalPaid = kitty.Ledger.Transactions.Where(t => t.Person.Email == lsl.Person.Email && t.TransactionType == TransactionType.Payment).Sum(t => t.TransactionAmount);
            lsl.TotalOwed = lsl.Purchases.Sum(p => p.ProductTotal);            
            lsl.TotalAdjustments = kitty.Ledger.Transactions.Where(t => t.Person.Email == lsl.Person.Email && t.TransactionType == TransactionType.Adjustment).Sum(t => t.TransactionAmount);
            lsl.Balance = lsl.TotalPaid - lsl.TotalOwed;

            return kitty;
        }
        private IEnumerable<StockItem> ReplaceStockItem(IEnumerable<StockItem> enumerable, StockItem value)
        {
            return enumerable.Select(x => x.MainName == value.MainName ? value : x);
        }
        private IEnumerable<StockItem> RemoveStockItem(IEnumerable<StockItem> enumerable, string stockItemName)
        {
            return enumerable.Where((x) => x.MainName != stockItemName);
        }

        public async Task RenameMember(string email, string newName)
        {
            var kitty = await _dataStore.GetKitty(Kitty.Id);

            var me = new Member
            {
                Email = email,
                DisplayName = newName
            };

            foreach (var t in kitty.Ledger.Transactions)
            {
                if (t.Person.Email == me.Email)
                {
                    t.Person = me;
                }
            }
            foreach (var s in kitty.Ledger.Summary)
            {
                if (s.Person.Email == me.Email)
                {
                    s.Person = me;
                }
            }

            await _dataStore.SaveKittyToDb(kitty);
            Kitty = kitty;
        }

        public async Task TickMultiplePeople(List<string> people, StockItem item, int count)
        {
            var kitty = await _dataStore.GetKitty(Kitty.Id);
            List<string> emails = new List<string>(people.Count());
            foreach (var p in people)
            {
                var summ = kitty.Ledger.Summary.FirstOrDefault(s => s.Person.DisplayName == p);
                emails.Add(summ.Person.Email);
                kitty.Ledger.Transactions.Add(new Transaction
                {
                    Date = DateTime.Now,
                    Person = summ.Person,
                    TransactionType = TransactionType.Purchase,
                    TransactionAmount = item.SalePrice * count,
                    TransactionCount = count,
                    TransactionName = item.MainName
                });
            }

            foreach (var e in emails)
            {
                kitty = RecalculateLedgerSummary(kitty, e);
            }
            await _dataStore.SaveKittyToDb(kitty);
            Kitty = kitty;
        }

        public async Task MakePayment(string email, decimal amount)
        {
            var kitty = await _dataStore.GetKitty(Kitty.Id);
                        
            var me = kitty.Ledger.Summary.FirstOrDefault(lsl => lsl.Person.Email == email).Person;

            kitty.Ledger.Transactions.Add(new Transaction
            {
                Date = DateTime.Now,
                Person = me,
                TransactionType = TransactionType.Payment,
                TransactionAmount = amount,
                TransactionCount = 1,
                TransactionName = "Cash"
            });

            kitty = RecalculateLedgerSummary(kitty, email);
            await _dataStore.SaveKittyToDb(kitty);
            Kitty = kitty;

        }
        public async Task AdjustBalanceBy(string email, decimal amount)
        {
            var kitty = await _dataStore.GetKitty(Kitty.Id);

            var me = kitty.Ledger.Summary.FirstOrDefault(lsl => lsl.Person.Email == email).Person;

            kitty.Ledger.Transactions.Add(new Transaction
            {
                Date = DateTime.Now,
                Person = me,
                TransactionType = TransactionType.Adjustment,
                TransactionAmount = amount,
                TransactionCount = 1,
                TransactionName = "Cash"
            });

            kitty = RecalculateLedgerSummary(kitty, email);
            await _dataStore.SaveKittyToDb(kitty);
            Kitty = kitty;

        }

        public async Task ProvideStock(string email, StockItem sItem)
        {
            var kitty = await _dataStore.GetKitty(Kitty.Id);

            var sl = kitty.Ledger.Summary.FirstOrDefault(lsl => lsl.Person.Email == email);
            var me = sl.Person;
            var iName = sItem.StockGrouping + " of " + sItem.MainName;
            kitty.Ledger.Transactions.Add(new Transaction
            {
                Date = DateTime.Now,
                Person = me,
                TransactionType = TransactionType.Provision,
                TransactionCount = 1,
                TransactionName = iName
            });

            sl.Provisions.TryGetValue(iName, out var prv);
            sl.Provisions[iName] = prv + 1;
            
            kitty = RecalculateLedgerSummary(kitty, email);
            await _dataStore.SaveKittyToDb(kitty);
            Kitty = kitty;

        }

        public string GetKittyBalance()
        {
            return string.Format("{0:#.00}", Kitty.Ledger.Summary.Sum(lsl => lsl.TotalOwed) + Kitty.Ledger.Summary.Sum(lsl => lsl.TotalAdjustments));
        }

        public string GetKittyOnHand()
        {
            return string.Format("{0:#.00}", Kitty.Ledger.Summary.Sum(lsl => lsl.TotalPaid) + Kitty.Ledger.Summary.Sum(lsl => lsl.TotalAdjustments));
        }

        public async Task CombineUsers(string keepUserEmail, string absorbUserEmail)
        {
            var kitty = await _dataStore.GetKitty(Kitty.Id);

            kitty.Ledger.Summary = kitty.Ledger.Summary.Where(lsl => lsl.Person.Email != absorbUserEmail).ToList();

            var person = kitty.Ledger.Summary.First(lsl => lsl.Person.Email == keepUserEmail).Person;

            foreach(var t in kitty.Ledger.Transactions.Where(tr => tr.Person.Email == absorbUserEmail))
            {
                t.Person = person;
            }
            RecalculateLedgerSummary(kitty, keepUserEmail);

            await _dataStore.SaveKittyToDb(kitty);
            Kitty = kitty;
        }
    }
}
