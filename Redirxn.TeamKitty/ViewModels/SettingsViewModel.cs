using Redirxn.TeamKitty.Services.Gateway;
using Redirxn.TeamKitty.Services.Identity;
using Splat;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Redirxn.TeamKitty.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        IKittyService _kittyService;

        public SettingsViewModel(IKittyService kittyService = null)
        {
            _kittyService = kittyService ?? Locator.Current.GetService<IKittyService>();
        }

        internal Task<string> GetKittyJoinCode()
        {
            return _kittyService.GetJoinCode();
        }

        internal Task JoinKittyWithCode(string joinCode)
        {
            return _kittyService.JoinKittyWithCode(joinCode);
        }
    }
}
