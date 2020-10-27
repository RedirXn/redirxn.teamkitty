using Redirxn.TeamKitty.Models;
using Splat;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Redirxn.TeamKitty.Services.Gateway
{
    public class KittyService : IKittyService
    {
        public Kitty Kitty { get; set; }

        IDataStore _dataStore;

        public KittyService(IDataStore dataStore = null)
        {
            _dataStore = dataStore ?? Locator.Current.GetService<IDataStore>();
        }

        public async Task<Kitty> LoadKitty(string kittyName)
        {
            Kitty = await _dataStore.GetKitty(kittyName ?? string.Empty);
            if (Kitty == null)
            {
                Kitty = new Kitty();
            }                        
            return Kitty;
        }

        public async Task SaveStockItem(string kittyId, StockItem stockItem)
        {
            Kitty = await _dataStore.SaveStockItem(kittyId, stockItem);            
        }

        public async Task DeleteStockItem(string kittyId, string stockName)
        {
            Kitty = await _dataStore.DeleteStockItem(kittyId, stockName);
        }

        public async Task CreateNewKitty(NetworkAuthData loginData, UserInfo userDetail, string newKittyName)
        {
            Kitty = await _dataStore.CreateNewKitty(loginData, userDetail, newKittyName);
        }
    }
}
