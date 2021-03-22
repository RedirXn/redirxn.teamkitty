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

        public ICommand OnLoginCommand { get; set; }
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

            OnLoginCommand = new Command(async () => await LoginAsync());
        }

        async Task LoginAsync()
        {
            try
            {
                IsLoading = true;
                CanClick = false;

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
                CanClick = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                await _dialogService.Alert("Error", "An Error Occurred", "OK");
                IsLoading = false;
                CanClick = true;
            }
        }


    }
}
