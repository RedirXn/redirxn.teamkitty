using Redirxn.TeamKitty.Models;
using Splat;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public async Task<Kitty> LoadKitty(string kittyId)
        {
            Kitty = await _dataStore.GetKitty(kittyId ?? string.Empty);
            if (Kitty == null)
            {
                Kitty = new Kitty();
            }                        
            return Kitty;
        }

        public async Task SaveStockItem(StockItem stockItem)
        {
            Kitty = await _dataStore.SaveStockItem(Kitty.Id, stockItem);            
        }

        public async Task DeleteStockItem(string stockName)
        {
            Kitty = await _dataStore.DeleteStockItem(Kitty.Id, stockName);
        }

        public async Task CreateNewKitty(NetworkAuthData loginData, UserInfo userDetail, string newKittyName)
        {
            Kitty = await _dataStore.CreateNewKitty(loginData, userDetail, newKittyName);
        }

        public async Task<string> GetJoinCode()
        {
            return await _dataStore.ResetJoinCode(Kitty.Id);
        }

        public async Task JoinKittyWithCode(NetworkAuthData loginData, UserInfo userDetail, string joinCode)
        {
            var k = await _dataStore.JoinKittyWithCode(loginData, userDetail, joinCode);
            if (k != null)
            {
                Kitty = k;
            }
        }

        public bool AmIAdmin(string email)
        {
            return Kitty.Administrators.Any(s => s.Equals(email, StringComparison.OrdinalIgnoreCase));
        }
    }
}
