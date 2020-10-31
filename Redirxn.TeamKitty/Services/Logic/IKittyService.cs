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
        Task<string> CreateNewKitty(string email, string newKittyName);
        bool AmIAdmin(string email);
        Task AddNewUser(string newUser);
        Task TickMeASingle(string email, string personDisplayName, StockItem item);
        Task AddRegisteredUser(string email, string name, string kittyId = null);
    }
}
