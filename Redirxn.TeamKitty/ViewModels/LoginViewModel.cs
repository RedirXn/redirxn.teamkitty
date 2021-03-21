using Newtonsoft.Json;
using Plugin.FacebookClient;
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

        public ICommand OnLoginWithFacebookCommand { get; set; }
        public bool _canClick = true;
        public bool CanClick
        {
            get { return _canClick; }
            set { SetProperty(ref _canClick, value); }
        }
        public bool _isLoading = false;
        private App _app;

        public bool IsLoading
        {
            get { return _isLoading; }
            set { SetProperty(ref _isLoading, value); }
        }

        public LoginViewModel(App app, IRoutingService navigationService = null, IIdentityService identityService = null, IDialogService dialogService = null)
        {
            this._app = app;
            _navigationService = navigationService ?? Locator.Current.GetService<IRoutingService>();
            _identityService = identityService ?? Locator.Current.GetService<IIdentityService>();
            _dialogService = dialogService ?? Locator.Current.GetService<IDialogService>();

            OnLoginWithFacebookCommand = new Command(async () => await LoginFacebookAsync());
        }

        async Task LoginFacebookAsync()
        {
            try
            {
                IsLoading = true;

                var loginDetail = await DependencyService.Get<ILoginProvider>().GetEmailByLoggingIn();

                var socialLoginData = new NetworkAuthData
                {
                    Email = loginDetail.Item2.Id,
                    Name = loginDetail.Item2.Name,
                    Id = loginDetail.Item2.Id
                };
                await _identityService.Init(loginDetail.Item1, socialLoginData);
                await _navigationService.NavigateTo("///main/home");

                IsLoading = false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                await _dialogService.Alert("Error", "An Error Occurred", "OK");
                IsLoading = false;
            }
        }


    }
}
