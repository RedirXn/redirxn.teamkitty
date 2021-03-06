﻿using Newtonsoft.Json;
using Redirxn.TeamKitty.Models;
using Redirxn.TeamKitty.Services.Application;
using Redirxn.TeamKitty.Services.Logic;
using Splat;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;


namespace Redirxn.TeamKitty.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private IRoutingService _navigationService;
        private IIdentityService _identityService;
        private IDialogService _dialogService;

        public ICommand OnLoginCommand { get; set; }
        public ICommand OnClearLoginCommand { get; set; }

        public bool _canClick = true;
        public bool CanClick
        {
            get { return _canClick; }
            set { SetProperty(ref _canClick, value); }
        }
        public bool _isLoading = false;

        public bool IsLoading
        {
            get { return _isLoading; }
            set { SetProperty(ref _isLoading, value); }
        }
        internal async Task StartLogin()
        {
            IsLoading = true;
            CanClick = false;

            await Task.Run(() => { });
            
            string savedLoginData = GetStringAppProperty("LoginData");
            string savedAccessToken = GetStringAppProperty("AccessToken");
            string savedRefreshToken = GetStringAppProperty("RefreshToken");
            
            bool hasSavedCreds = (!string.IsNullOrWhiteSpace(savedLoginData) &&
                !string.IsNullOrWhiteSpace(savedAccessToken));

            if (hasSavedCreds)
            {
                var socialLoginData = JsonConvert.DeserializeObject<NetworkAuthData>(savedLoginData);
                var token = savedAccessToken;
                await _identityService.Init(token, socialLoginData);
                if (_identityService.HasDataCredentials)
                {
                    await _navigationService.NavigateTo("///main/home");
                    return;
                }

                if (!string.IsNullOrEmpty(savedRefreshToken))
                {
                    var newIdToken = await DependencyService.Get<ILoginProvider>().GetIdFromRefresh(savedRefreshToken);
                    await _identityService.Init(newIdToken, socialLoginData);
                    if (_identityService.HasDataCredentials)
                    {
                        await Xamarin.Essentials.SecureStorage.SetAsync("AccessToken", newIdToken);
                        await _navigationService.NavigateTo("///main/home");
                        return;
                    }
                }
                await Xamarin.Essentials.SecureStorage.SetAsync("AccessToken", string.Empty);
            }

            await LoginAsync();

            IsLoading = false;
            CanClick = true;
        }

        private static string GetStringAppProperty(string propertyName)
        {
            return Xamarin.Essentials.SecureStorage.GetAsync(propertyName).Result ?? string.Empty;
        }

        public LoginViewModel(IRoutingService navigationService = null, IIdentityService identityService = null, IDialogService dialogService = null)
        {
            _navigationService = navigationService ?? Locator.Current.GetService<IRoutingService>();
            _identityService = identityService ?? Locator.Current.GetService<IIdentityService>();
            _dialogService = dialogService ?? Locator.Current.GetService<IDialogService>();

            OnLoginCommand = new Command(async () => await StartLogin());
            OnClearLoginCommand = new Command(async () => await ClearSavedLogin());
        }

        private async Task ClearSavedLogin()
        {
            await Xamarin.Essentials.SecureStorage.SetAsync("LoginData", string.Empty);
            await Xamarin.Essentials.SecureStorage.SetAsync("AccessToken", string.Empty);
            await Xamarin.Essentials.SecureStorage.SetAsync("RefreshToken", string.Empty);
        }

        async Task LoginAsync()
        {
            try
            {
                IsLoading = true;
                CanClick = false;

                var loginDetail = await DependencyService.Get<ILoginProvider>().GetEmailByLoggingIn();

                if (loginDetail == null)
                {
                    IsLoading = false;
                    CanClick = true;
                    return;
                }
                var socialLoginData = new NetworkAuthData
                {
                    Email = loginDetail.Item2.Id,
                    Name = loginDetail.Item2.Name,
                    Id = loginDetail.Item2.Id
                };

                await Xamarin.Essentials.SecureStorage.SetAsync("LoginData", JsonConvert.SerializeObject(socialLoginData));
                await Xamarin.Essentials.SecureStorage.SetAsync("AccessToken", loginDetail.Item1.Item1);
                await Xamarin.Essentials.SecureStorage.SetAsync("RefreshToken", loginDetail.Item1.Item2 ?? string.Empty);

                await _identityService.Init(loginDetail.Item1.Item1, socialLoginData);
                if (_identityService.HasDataCredentials)
                {
                    await _navigationService.NavigateTo("///main/home");
                    await Task.Run(() => { });
                    return;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                await _dialogService.Alert("Error", "An Error Occurred", "OK");
            }
            IsLoading = false;
            CanClick = true;
        }


    }
}
