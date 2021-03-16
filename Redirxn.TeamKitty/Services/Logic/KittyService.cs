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
            Kitty kitty = await GetUnlockedKitty();

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
            Kitty kitty = await GetUnlockedKitty();

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
            Kitty kitty = await GetUnlockedKitty(kittyId);

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

        public async Task TickMeASingle(string email, string name, StockItem item)
        {
            Kitty kitty = await GetUnlockedKitty();

            var dName = kitty.Ledger.Summary.FirstOrDefault(lsl => lsl.Person.Email == email)?.Person.DisplayName;

            TickSomeoneASingle(kitty, email, dName ?? name, item);

            await _dataStore.SaveKittyToDb(kitty);
            Kitty = kitty;
        }

        private async Task<Kitty> GetUnlockedKitty(string kittyId)
        {
            var kitty = await _dataStore.GetKitty(kittyId);
            if (kitty.KittyConfig.Locked) { throw new ApplicationException("Cannot change a Kitty that is locked"); }
            return kitty;
        }
        private async Task<Kitty> GetUnlockedKitty()
        {
            return await GetUnlockedKitty(Kitty.Id);
        }

        private void TickSomeoneASingle(Kitty kitty, string email, string personDisplayName, StockItem item)
        {
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
        }

        private Kitty RecalculateLedgerSummary(Kitty kitty, string email)
        {
            var lsl = kitty.Ledger.Summary.FirstOrDefault(s => s.Person.Email == email);
            lsl.Purchases = new List<Purchase>();
            lsl.Provisions = new Dictionary<string, int>();
            lsl.PurchaseText = "";
            lsl.ProvisionText = "";
            foreach (var si in kitty.KittyConfig.StockItems)
            {
                var filtered = kitty.Ledger.Transactions.Where(t =>
                        t.Person.Email == lsl.Person.Email &&                        
                        t.TransactionName == si.MainName
                        );
                var filteredPurchases = filtered.Where(t => t.TransactionType == TransactionType.Purchase);
                var filteredProvisions = filtered.Where(t => t.TransactionType == TransactionType.Provision);
                var pCount = filteredPurchases.Sum(t => t.TransactionCount);
                var pvCount = filteredProvisions.Sum(t => t.TransactionCount);

                lsl.Purchases.Add(new Purchase
                {
                    ProductName = si.MainName,
                    ProductCount = pCount,
                    ProductTotal = filteredPurchases.Sum(t => t.TransactionAmount)
                });
                lsl.PurchaseText += (!string.IsNullOrEmpty(lsl.PurchaseText) ? "  " : string.Empty) + pCount + " " + si.PluralName;

                lsl.Provisions[si.MainName] = pvCount;
                lsl.ProvisionText += (!string.IsNullOrEmpty(lsl.ProvisionText) ? "  " : string.Empty) + pvCount + " " + si.StockGrouping + " of " + si.MainName;
            }
            lsl.TotalPaid = kitty.Ledger.Transactions.Where(t => t.Person.Email == lsl.Person.Email && (t.TransactionType == TransactionType.Payment || t.TransactionType == TransactionType.CarryOver)).Sum(t => t.TransactionAmount);
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
            // Only supports renaming self / app users
            // Non App users should create new and combine
            Kitty kitty = await GetUnlockedKitty();

            if (kitty.Ledger.Summary.FirstOrDefault(lsl => lsl.Person.DisplayName == newName) != null)
            {
                throw new ApplicationException("That name is already in use.");
            }

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
            Kitty kitty = await GetUnlockedKitty();
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
            Kitty kitty = await GetUnlockedKitty();

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
            Kitty kitty = await GetUnlockedKitty();

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
            Kitty kitty = await GetUnlockedKitty();

            var sl = kitty.Ledger.Summary.FirstOrDefault(lsl => lsl.Person.Email == email);
            var me = sl.Person;
            kitty.Ledger.Transactions.Add(new Transaction
            {
                Date = DateTime.Now,
                Person = me,
                TransactionType = TransactionType.Provision,
                TransactionCount = 1,
                TransactionName = sItem.MainName
            });
            
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
            Kitty kitty = await GetUnlockedKitty();

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
        public async Task RecalculateKitty()
        {
            Kitty kitty = await GetUnlockedKitty();
            foreach (var lsl in kitty.Ledger.Summary)
            {
                RecalculateLedgerSummary(kitty, lsl.Person.Email);
            }
            await _dataStore.SaveKittyToDb(kitty);
            Kitty = kitty;
        }

        public async Task StartTakingOrdersInSession(string userDisplay)
        {
            Kitty kitty = await GetUnlockedKitty();

            if (kitty.Session == null || !kitty.Session.IsStarted || !string.IsNullOrWhiteSpace(kitty.Session.PersonTakingOrders))
            {
                return;
            }

            kitty.Session.PersonTakingOrders = userDisplay;

            await _dataStore.SaveKittyToDb(kitty);
            Kitty = kitty;
        }

        public async Task StartSession()
        {
            Kitty kitty = await GetUnlockedKitty();

            if (kitty.Session == null || !kitty.Session.IsStarted)
            {
                var sess = new Session { 
                    IsStarted = true,
                    Orders = new List<SessionOrder>(),
                    OrderOptions = kitty.Session.OrderOptions
                };

                kitty.Session = sess;
                await _dataStore.SaveKittyToDb(kitty);
                Kitty = kitty;
            }
        }
        public async Task EndSession()
        {
            Kitty kitty = await GetUnlockedKitty();

            if (kitty.Session.IsStarted)
            {
                var sess = new Session
                {
                    IsStarted = false,
                    Orders = new List<SessionOrder>(),
                    OrderOptions = kitty.Session.OrderOptions
                };

                kitty.Session = sess;
                await _dataStore.SaveKittyToDb(kitty);
                Kitty = kitty;
            }
        }

        public async Task OrderItemInSession(string userId, string displayName, string stockItem, string option)
        {
            Kitty kitty = await GetUnlockedKitty();

            if (string.IsNullOrWhiteSpace(kitty.Session.PersonTakingOrders))
            {
                return;
            }
            var os = kitty.Session.Orders.ToList();
            os.RemoveAll(o => o.PersonId == userId);
            kitty.Session.Orders = os;
            kitty.Session.Orders.Add(new SessionOrder
            {
                PersonId = userId,
                PersonDisplayName = displayName,
                StockItemName = stockItem,
                OptionName = option,
                IsDelivered = false
            });

            await _dataStore.SaveKittyToDb(kitty);
            Kitty = kitty;
        }

        public async Task CancelOrderInSession(string userId)
        {
            Kitty kitty = await GetUnlockedKitty();

            var os = kitty.Session.Orders.ToList();
            os.RemoveAll(o => o.PersonId == userId);
            kitty.Session.Orders = os;

            await _dataStore.SaveKittyToDb(kitty);
            Kitty = kitty;
        }

        public async Task ClearAllOpenOrdersInSession()
        {
            Kitty kitty = await GetUnlockedKitty();

            kitty.Session.Orders = new List<SessionOrder>();
            kitty.Session.PersonTakingOrders = string.Empty;

            await _dataStore.SaveKittyToDb(kitty);
            Kitty = kitty;
        }

        public async Task CloseOrderTakingInSession()
        {
            Kitty kitty = await GetUnlockedKitty();

            kitty.Session.PersonTakingOrders = string.Empty;

            await _dataStore.SaveKittyToDb(kitty);
            Kitty = kitty;
        }

        public async Task ReceivedItemIsSession(string userId)
        {
            Kitty kitty = await GetUnlockedKitty();

            var i = kitty.Session.Orders.Single(o => o.PersonId == userId);
            var si = kitty.KittyConfig.StockItems.Single(s => s.MainName == i.StockItemName);
            
            TickSomeoneASingle(kitty, i.PersonId, i.PersonDisplayName, si);
            var os = kitty.Session.Orders.ToList();
            os.RemoveAll(o => o.PersonId == userId);
            kitty.Session.Orders = os;

            await _dataStore.SaveKittyToDb(kitty);
            Kitty = kitty;
        }
        public string GetOrderListText()
        {
            string orderList = string.Empty;
            foreach (var si in Kitty.KittyConfig.StockItems)
            {
                if (!string.IsNullOrEmpty(orderList))
                {
                    orderList += Environment.NewLine;
                }
                var list = Kitty.Session.Orders.Where(o => o.StockItemName == si.MainName);
                orderList += si.MainName + " : " + list.Count();
                if (list.Count() > 0)
                {
                    foreach(var o in list)
                    {
                        orderList += Environment.NewLine + o.PersonDisplayName;
                    }
                }
            }
            return orderList;           
        }

        public async Task CombineKitties(string oldKittyId, string newKittyId)
        {
            var kitty1 = await GetUnlockedKitty(oldKittyId);
            var kitty2 = await GetUnlockedKitty(newKittyId);

            kitty1.KittyConfig.Locked = true;
            await _dataStore.SaveKittyToDb(kitty1);            

            foreach (var lsl in kitty1.Ledger.Summary)
            {
                var newPerson = kitty2.Ledger.Summary.FirstOrDefault(l => l.Person.Email == lsl.Person.Email)?.Person;
                if (newPerson == null)
                {
                    kitty2.Ledger.Summary.Add(new LedgerSummaryLine
                    {
                        Person = lsl.Person,
                        Balance = 0M,
                        TotalOwed = 0M,
                        TotalPaid = 0M
                    });
                    newPerson = lsl.Person;
                }
                
                kitty2.Ledger.Transactions.Add(new Transaction
                {
                    Date = DateTime.Now,
                    Person = newPerson,
                    TransactionType = TransactionType.CarryOver,
                    TransactionAmount = lsl.Balance,
                    TransactionName = kitty1.DisplayName
                });
                RecalculateLedgerSummary(kitty2, newPerson.Email);

                kitty1.Ledger.Transactions.Add(new Transaction
                {
                    Date = DateTime.Now,
                    Person = lsl.Person,
                    TransactionType = TransactionType.CarryOver,
                    TransactionAmount = -(lsl.Balance),
                    TransactionName = kitty2.DisplayName
                });
                RecalculateLedgerSummary(kitty1, lsl.Person.Email);
            }
            
            await _dataStore.SaveKittyToDb(kitty2);
            await _dataStore.SaveKittyToDb(kitty1);
            Kitty = kitty2;
        }

        public Tuple<string, string>[] GetNonAdminAppUsers()
        {
            return Kitty.Ledger.Summary.Where(lsl => lsl.Person.Email != lsl.Person.DisplayName && !Kitty.Administrators.Contains(lsl.Person.Email))
                .Select(lsl => Tuple.Create(lsl.Person.Email, lsl.Person.DisplayName)).ToArray();
        }

        public async Task MakeUserAdmin(string adminUser)
        {
            var kitty = await _dataStore.GetKitty(Kitty.Id);

            var admins = kitty.Administrators.ToList();
            admins.Add(adminUser);
            kitty.Administrators = admins;

            await _dataStore.SaveKittyToDb(kitty);
            Kitty = kitty;

        }
    }
}
