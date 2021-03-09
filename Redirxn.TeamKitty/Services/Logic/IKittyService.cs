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
        Task AddRegisteredUser(string email, string name, string kittyId = null);
        Task RenameMember(string email, string newName);
        Task TickMeASingle(string email, string name, StockItem stockItem);
        Task TickMultiplePeople(List<string> people, StockItem stockItem, int count);
        Task MakePayment(string email, decimal amount);
        Task AdjustBalanceBy(string email, decimal amount);
        Task ProvideStock(string email, StockItem sItem);
        string GetKittyBalance();
        string GetKittyOnHand();
        Task CombineUsers(string keepUserEmail, string absorbUserEmail);
        Task RecalculateKitty();
        Task StartTakingOrdersInSession(string userDisplay);
        Task StartSession();
        Task OrderItemInSession(string userId, string displayName, string stockItem, string option);
        Task CancelOrderInSession(string userId);
        Task ClearAllOpenOrdersInSession();
        Task CloseOrderTakingInSession();
        Task ReceivedItemIsSession(string userId);
        Task EndSession();
        string GetOrderListText();
        Task CombineKitties(string oldKittyId, string newKittyId);
        Tuple<string, string>[] GetNonAdminAppUsers();
        Task MakeUserAdmin(string adminUser);
    }
}
