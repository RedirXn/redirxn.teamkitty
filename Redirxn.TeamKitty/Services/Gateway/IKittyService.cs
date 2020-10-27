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

        Task<Kitty> LoadKitty(string defaultKitty);
        Task SaveStockItem(string kittyId, StockItem stockItem);
        Task DeleteStockItem(string kittyId, string stockName);
        Task CreateNewKitty(NetworkAuthData loginData, UserInfo userDetail, string newKittyName);
    }
}
