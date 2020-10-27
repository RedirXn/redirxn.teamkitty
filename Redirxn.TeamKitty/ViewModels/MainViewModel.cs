using Plugin.FacebookClient;
using Redirxn.TeamKitty.Models;
using Redirxn.TeamKitty.Services.Gateway;
using Redirxn.TeamKitty.Services.Identity;
using Splat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Redirxn.TeamKitty.ViewModels
{
    class MainViewModel : BaseViewModel
    {        
        private IIdentityService _identityService;
        private IKittyService _kittyService;

        string _currentKitty = string.Empty;
        public string CurrentKitty
        {
            get { return _currentKitty; }
            set { SetProperty(ref _currentKitty, value); }
        }

        public MainViewModel(IIdentityService identityService = null, IKittyService kittyService = null)
        {            
            _identityService = identityService ?? Locator.Current.GetService<IIdentityService>();
            _kittyService = kittyService ?? Locator.Current.GetService<IKittyService>();            
        }

        internal async Task Init()
        {
            await _kittyService.LoadKitty(_identityService.UserDetail.DefaultKitty);
            CurrentKitty = _kittyService.Kitty?.DisplayName;
        }

        internal async Task CreateNewKitty(string newKittyName)
        {
            if (!_identityService.KittyNameExists(newKittyName))
            {                
                await _kittyService.CreateNewKitty(_identityService.LoginData, _identityService.UserDetail, newKittyName);
                await _identityService.ReloadUserDetail();
                CurrentKitty = _kittyService.Kitty.DisplayName;                
            }
        }
    }
}
