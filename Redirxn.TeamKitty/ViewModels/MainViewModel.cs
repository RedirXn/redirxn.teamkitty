using Plugin.FacebookClient;
using Redirxn.TeamKitty.Services.Gateway;
using Redirxn.TeamKitty.Services.Identity;
using Redirxn.TeamKitty.Views;
using Splat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Redirxn.TeamKitty.ViewModels
{    
    class MainViewModel : BaseViewModel
    {
        private IDataStore _dataStore;
        private IIdentityService _identityService;

        string _currentKitty = string.Empty;
        public string CurrentKitty
        {
            get { return _currentKitty; }
            set { SetProperty(ref _currentKitty, value); }
        }

        public IEnumerable<string> Kitties;

        public MainViewModel(IDataStore dataStore = null, IIdentityService identityService = null)
        {
            this._dataStore = dataStore ?? Locator.Current.GetService<IDataStore>();
            this._identityService = identityService ?? Locator.Current.GetService<IIdentityService>();
        }

        // Called by the views OnAppearing method
        internal async Task<bool> Init()
        {
            this._dataStore.Init(CrossFacebookClient.Current.ActiveToken);   

            // Load data from AWS, set properties in the view model that are bound to the view.
            if (_identityService.UserDetail == null)
            {
                _identityService.UserDetail = await _dataStore.GetUserDetail(_identityService.LoginData.Email);
            }            
            if (_identityService.UserDetail != null && _identityService.UserDetail.DefaultKitty != null)
            {
                CurrentKitty = _identityService.UserDetail.DefaultKitty;
            }
            return true;
        }

        internal async void CreateNewKitty(string newKittyName)
        {
            if (Kitties == null || !Kitties.Contains(newKittyName))
            {
                _identityService.UserDetail = await _dataStore.CreateNewKitty(_identityService.LoginData, _identityService.UserDetail, newKittyName);
                CurrentKitty = _identityService.UserDetail.DefaultKitty;
            }
        }
    }
}
