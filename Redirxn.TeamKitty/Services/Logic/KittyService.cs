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

        public async Task<string> CreateNewKitty(string email, string newKittyName)
        {
            var kitty = new Kitty
            {
                Id = email + '|' + newKittyName,
                Administrators = new [] { email }
            };

            await _dataStore.SaveKittyToDb(kitty);
            Kitty = kitty;
            return Kitty.Id;
        }
        public async Task AddNewUser(string name)
        {
            await AddUser(name);
        }
        public async Task AddRegisteredUser(string email, string name)
        {
            await AddUser(name, email);
        }
        public async Task AddUser(string name, string email = null)
        {
            var kitty = await _dataStore.GetKitty(Kitty.Id);

            if ((email == null && kitty.Ledger.Summary.Any(m => m.Person.DisplayName == name))
                || (kitty.Ledger.Summary.Any(m => m.Person.Email == email)))
            {
                return;
            }

            kitty.Ledger.Summary.Add(new LedgerSummaryLine
            {
                Person = new Member { DisplayName = name, Email = email ?? "*" },
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

            kitty = await RecalculateLedgerSummary(kitty);
        }

        private Task<Kitty> RecalculateLedgerSummary(Kitty kitty)
        {
            throw new NotImplementedException(); // TODO
        }
        private IEnumerable<StockItem> ReplaceStockItem(IEnumerable<StockItem> enumerable, StockItem value)
        {
            return enumerable.Select(x => x.MainName == value.MainName ? value : x);
        }
        private IEnumerable<StockItem> RemoveStockItem(IEnumerable<StockItem> enumerable, string stockItemName)
        {
            return enumerable.Where((x) => x.MainName != stockItemName);
        }


    }
}
