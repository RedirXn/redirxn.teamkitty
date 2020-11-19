using Redirxn.TeamKitty.Models;
using Redirxn.TeamKitty.Services.Gateway;
using Splat;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Redirxn.TeamKitty.Services.Logic
{
    public class IdentityService : IIdentityService
    {
        IUserDataStore _dataStore;
        public bool IsUserLoggedIn { get { return LoginData != null && LoginData.Email != string.Empty; } }
        public NetworkAuthData LoginData { get; set; }
        public UserInfo UserDetail { get; set; }        

        public IdentityService(IUserDataStore dataStore = null)
        {
            _dataStore = dataStore ?? Locator.Current.GetService<IUserDataStore>();
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
        public async Task AddMeToKitty(string kittyId)
        {

            if (UserDetail == null || string.IsNullOrEmpty(UserDetail.Id))
            {
                UserDetail = new UserInfo { Id = LoginData.Email, Name = LoginData.Name, KittyNames = new List<string> { kittyId }, DefaultKitty = kittyId };
            }
            else
            {
                UserDetail.KittyNames.Add(kittyId);
                UserDetail.DefaultKitty = kittyId;
            }

            await _dataStore.SaveUserDetailToDb(UserDetail);
        }

        public async Task Rename(string newName)
        {
            UserDetail.Name = newName;
            await _dataStore.SaveUserDetailToDb(UserDetail);
        }

        public async Task SetDefaultKitty(string nextKitty)
        {
            UserDetail.DefaultKitty = nextKitty;
            await _dataStore.SaveUserDetailToDb(UserDetail);
        }
    }

}
