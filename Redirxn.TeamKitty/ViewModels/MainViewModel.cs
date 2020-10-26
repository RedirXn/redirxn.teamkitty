using Plugin.FacebookClient;
using Redirxn.TeamKitty.Models;
using Redirxn.TeamKitty.Services.Gateway;
using Redirxn.TeamKitty.Services.Identity;
using Splat;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Redirxn.TeamKitty.ViewModels
{
    class MainViewModel : BaseViewModel
    {
        private IDataStore _dataStore;
        private IIdentityService _identityService;
        private IKittyService _kittyService;

        string _currentKitty = string.Empty;
        public string CurrentKitty
        {
            get { return _currentKitty; }
            set { SetProperty(ref _currentKitty, value); }
        }

        public IEnumerable<string> Kitties;

        public MainViewModel(IDataStore dataStore = null, IIdentityService identityService = null, IKittyService kittyService = null)
        {
            _dataStore = dataStore ?? Locator.Current.GetService<IDataStore>();
            _identityService = identityService ?? Locator.Current.GetService<IIdentityService>();
            _kittyService = kittyService ?? Locator.Current.GetService<IKittyService>();
        }

        // Called by the views OnAppearing method
        internal async Task<bool> Init()
        {
            _dataStore.Init(CrossFacebookClient.Current.ActiveToken);   

            // Load data from AWS, set properties in the view model that are bound to the view.
            if (_identityService.UserDetail == null)
            {
                _identityService.UserDetail = await _dataStore.GetUserDetail(_identityService.LoginData.Email);
            }            
            if (_identityService.UserDetail != null && _identityService.UserDetail.DefaultKitty != null)
            {
                CurrentKitty = _identityService.UserDetail.DefaultKitty.Split('|')[1];
                _kittyService.Kitty = await _dataStore.GetKitty(_identityService.UserDetail.DefaultKitty);
            }
            return true;
        }

        internal async void CreateNewKitty(string newKittyName)
        {
            if (Kitties == null || !Kitties.Contains(newKittyName))
            {
                _identityService.UserDetail = await _dataStore.CreateNewKitty(_identityService.LoginData, _identityService.UserDetail, newKittyName);
                CurrentKitty = _identityService.UserDetail.DefaultKitty.Split('|')[1];
                _kittyService.Kitty = new Kitty { Id = _identityService.UserDetail.DefaultKitty };
            }
        }
    }
}
