using Redirxn.TeamKitty.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Redirxn.TeamKitty.Services.Gateway
{
    public interface IKittyService
    {
        Kitty Kitty { get; set; }

        Task<Kitty> LoadKitty(string kittyId);
        Task SaveStockItem(StockItem stockItem);
        Task DeleteStockItem(string stockName);
        Task CreateNewKitty(NetworkAuthData loginData, UserInfo userDetail, string newKittyName);
        Task<string> GetJoinCode();
        Task JoinKittyWithCode(NetworkAuthData loginData, UserInfo userDetail, string joinCode);
        bool AmIAdmin(string email);
        Task AddNewUser(string newUser);
    }
}
