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

        internal async void OnAppearing()
        {
            bool isLoggedIn = (
                Application.Current.Properties.ContainsKey("IsLoggedIn") &&
                Convert.ToBoolean(Application.Current.Properties["IsLoggedIn"]) &&
                Application.Current.Properties.ContainsKey("LoginData") &&
                Application.Current.Properties.ContainsKey("Token"));
            if (!isLoggedIn)
            {
                await LoginAsync();
            }
            else
            {
                var socialLoginData = JsonConvert.DeserializeObject<NetworkAuthData>(Application.Current.Properties["LoginData"].ToString());
                var token = Application.Current.Properties["Token"].ToString();

                await _identityService.Init(token, socialLoginData);
                await _navigationService.NavigateTo("///main/home");
            }
        }

        public LoginViewModel(IRoutingService navigationService = null, IIdentityService identityService = null, IDialogService dialogService = null)
        {
            _navigationService = navigationService ?? Locator.Current.GetService<IRoutingService>();
            _identityService = identityService ?? Locator.Current.GetService<IIdentityService>();
            _dialogService = dialogService ?? Locator.Current.GetService<IDialogService>();
        }

        async Task LoginAsync()
        {
            try
            {
                var loginDetail = await DependencyService.Get<ILoginProvider>().GetEmailByLoggingIn();

                if (loginDetail == null)
                {
                    return;
                }
                var socialLoginData = new NetworkAuthData
                {
                    Email = loginDetail.Item2.Id,
                    Name = loginDetail.Item2.Name,
                    Id = loginDetail.Item2.Id
                };
                
                Application.Current.Properties["IsLoggedIn"] = Boolean.TrueString;
                Application.Current.Properties["Token"] = loginDetail.Item1;
                Application.Current.Properties["LoginData"] = JsonConvert.SerializeObject(socialLoginData);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                await _dialogService.Alert("Error", "An Error Occurred", "OK");
            }
        }


    }
}
