using Redirxn.TeamKitty.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Redirxn.TeamKitty.Services.Logic
{
    public interface IKittyService
    {
        Kitty Kitty { get; }
        Task LoadKitty(string kittyId);
        Task SaveStockItem(StockItem stockItem);
        Task DeleteStockItem(string stockName);
        Task<string> CreateNewKitty(string email, string userName, string newKittyName);
        bool AmIAdmin(string email);
        Task AddNewUser(string newUser);
        Task TickMeASingle(string email, string personDisplayName, StockItem item);
        Task AddRegisteredUser(string email, string name, string kittyId = null);
        Task RenameMember(string email, string newName);
        Task TickMultiplePeople(List<string> people, StockItem stockItem);
        Task MakePayment(string email, decimal amount);
        Task AdjustBalanceBy(string email, decimal amount);
        Task ProvideStock(string email, StockItem sItem);
        string GetKittyBalance();
        string GetKittyOnHand();
    }
}
