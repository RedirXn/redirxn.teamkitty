using Redirxn.TeamKitty.Services.Gateway;
using Redirxn.TeamKitty.Services.Identity;
using Splat;
using System;
using System.Threading.Tasks;

namespace Redirxn.TeamKitty.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        IKittyService _kittyService;
        IIdentityService _identityService;

        public bool IsAdmin { get; set; } = false;

        public SettingsViewModel(IKittyService kittyService = null, IIdentityService identityService = null)
        {
            _kittyService = kittyService ?? Locator.Current.GetService<IKittyService>();
            _identityService = identityService ?? Locator.Current.GetService<IIdentityService>();

            IsAdmin = _kittyService.AmIAdmin(_identityService.LoginData.Email);
        }

        internal Task<string> GetKittyJoinCode()
        {
            return _kittyService.GetJoinCode();
        }

        internal async Task JoinKittyWithCode(string joinCode)
        {
            await _kittyService.JoinKittyWithCode(_identityService.LoginData, _identityService.UserDetail, joinCode);
            await _identityService.ReloadUserDetail(); // TODO - notify that Kitty list has changed to do this.
        }

        internal async Task AddNewUser(string newUser)
        {
            await _kittyService.AddNewUser(newUser);
        }
    }
}
