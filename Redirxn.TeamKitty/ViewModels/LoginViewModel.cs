﻿using Newtonsoft.Json;
using Plugin.FacebookClient;
using Redirxn.TeamKitty.Models;
using Redirxn.TeamKitty.Services.Gateway;
using Redirxn.TeamKitty.Services.Identity;
using Redirxn.TeamKitty.Services.Routing;
using Redirxn.TeamKitty.Views;
using Splat;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace Redirxn.TeamKitty.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private IRoutingService _navigationService;
        private IIdentityService _identityService;
        private IDataStore _dataStore;
        IFacebookClient _facebookService = CrossFacebookClient.Current;

        public ICommand OnLoginWithFacebookCommand { get; set; }


        public LoginViewModel(IRoutingService navigationService = null, IIdentityService identityService = null, IDataStore dataStore = null)
        {
            _navigationService = navigationService ?? Locator.Current.GetService<IRoutingService>();
            _identityService = identityService ?? Locator.Current.GetService<IIdentityService>();
            _dataStore = dataStore ?? Locator.Current.GetService<IDataStore>();

            OnLoginWithFacebookCommand = new Command(async () => await LoginFacebookAsync());
        }

        async Task LoginFacebookAsync()
        {
            try
            {
                //if (_facebookService.IsLoggedIn)
                //{
                //    _facebookService.Logout();
                //}

                EventHandler<FBEventArgs<string>> userDataDelegate = null;

                userDataDelegate = async (object sender, FBEventArgs<string> e) =>
                {
                    if (e == null) return;

                    switch (e.Status)
                    {
                        case FacebookActionStatus.Completed:
                            var facebookProfile = await Task.Run(() => JsonConvert.DeserializeObject<FacebookProfile>(e.Data));
                            var socialLoginData = new NetworkAuthData
                            {
                                Email = facebookProfile.Email,
                                Name = $"{facebookProfile.FirstName} {facebookProfile.LastName}",
                                Id = facebookProfile.Id
                            };
                            _identityService.IsUserLoggedIn = true;
                            _identityService.LoginData = socialLoginData;
                            await _navigationService.NavigateTo("///main/home"); 
                            break;
                        case FacebookActionStatus.Canceled:
                            break;
                    }

                    _facebookService.OnUserData -= userDataDelegate;
                };

                _facebookService.OnUserData += userDataDelegate;

                string[] fbRequestFields = { "email", "first_name", "gender", "last_name" };
                string[] fbPermisions = { "email" };
                await _facebookService.RequestUserDataAsync(fbRequestFields, fbPermisions);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }


    }
}
