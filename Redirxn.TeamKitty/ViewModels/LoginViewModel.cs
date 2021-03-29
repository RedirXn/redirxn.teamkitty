using Newtonsoft.Json;
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

        public bool _canClick = false;
        public bool CanClick
        {
            get { return _canClick; }
            set { SetProperty(ref _canClick, value); }
        }
        public bool _isLoading = true;

        public bool IsLoading
        {
            get { return _isLoading; }
            set { SetProperty(ref _isLoading, value); }
        }
        internal async void OnAppearing()
        {
            string savedLoginData = GetStringAppProperty("LoginData");
            string savedAccessToken = GetStringAppProperty("AccessToken");
            string savedRefreshToken = GetStringAppProperty("RefreshToken");

            bool hasSavedCreds = (!string.IsNullOrWhiteSpace(savedLoginData) &&
                //!string.IsNullOrWhiteSpace(savedRefreshToken) &&
                !string.IsNullOrWhiteSpace(savedAccessToken));

            if (hasSavedCreds)
            {
                var socialLoginData = JsonConvert.DeserializeObject<NetworkAuthData>(savedLoginData);
                var token = savedAccessToken;
                await _identityService.Init(token, socialLoginData);
                if (!_identityService.HasDataCredentials)
                {
                    // get new access token from refresh token ????
                    // update saved credentials
                }
                else
                {
                    await _navigationService.NavigateTo("///main/home");
                    return;
                }
            }
            IsLoading = false;
            CanClick = true;
        }

        private static string GetStringAppProperty(string propertyName)
        {
            return Application.Current.Properties.ContainsKey(propertyName) ?
                Application.Current.Properties[propertyName].ToString() :
                string.Empty;
        }

        public LoginViewModel(IRoutingService navigationService = null, IIdentityService identityService = null, IDialogService dialogService = null)
        {
            _navigationService = navigationService ?? Locator.Current.GetService<IRoutingService>();
            _identityService = identityService ?? Locator.Current.GetService<IIdentityService>();
            _dialogService = dialogService ?? Locator.Current.GetService<IDialogService>();

            OnLoginCommand = new Command(async () => await LoginAsync());
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
                
                Application.Current.Properties["AccessToken"] = loginDetail.Item1;
                //Application.Current.Properties["RefreshToken"] = loginDetail.Item1;
                Application.Current.Properties["LoginData"] = JsonConvert.SerializeObject(socialLoginData);

                await _identityService.Init(loginDetail.Item1, socialLoginData);
                if (_identityService.HasDataCredentials)
                {
                    await _navigationService.NavigateTo("///main/home");
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
