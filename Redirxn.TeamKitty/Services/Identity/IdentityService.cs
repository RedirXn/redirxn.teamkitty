using Redirxn.TeamKitty.Models;
using Redirxn.TeamKitty.Services.Gateway;
using Splat;
using System;
using System.Collections.Generic;
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
        private string _token;

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
    }
}
