using Redirxn.TeamKitty.Models;
using Redirxn.TeamKitty.Services.Gateway;
using Splat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Redirxn.TeamKitty.Services.Identity
{   
    public class IdentityService : IIdentityService
    {
        IDataStore _dataStore;
        public bool IsUserLoggedIn { get { return LoginData != null && LoginData.Email != string.Empty; } }
        public NetworkAuthData LoginData { get; set; }
        public UserInfo UserDetail { get; set; }        

        public IdentityService(IDataStore dataStore = null)
        {
            _dataStore = dataStore ?? Locator.Current.GetService<IDataStore>();
        }
        
        public async Task Init(string activeToken, NetworkAuthData socialLoginData)
        {
            LoginData = socialLoginData;

            _dataStore.Init(activeToken);

            UserDetail = await _dataStore.GetUserDetail(LoginData.Email);
        }

        public async Task ReloadUserDetail()
        {
            UserDetail = await _dataStore.GetUserDetail(LoginData.Email);
        }

        public bool KittyNameExists(string newKittyName)
        {
            if (UserDetail == null || !UserDetail.KittyNames.Any(k => k.Contains("|" + newKittyName)))
            {
                return false;
            }
            return true;
        }
    }
}
