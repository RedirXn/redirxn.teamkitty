using Redirxn.TeamKitty.Services.Application;
using Redirxn.TeamKitty.Services.Logic;
using Splat;
using System;
using System.Diagnostics;

namespace Redirxn.TeamKitty.ViewModels
{
    class LoadingViewModel : BaseViewModel
    {
        private readonly IRoutingService _routingService;
        private readonly IIdentityService _identityService;
        private readonly IDialogService _dialogService;

        public LoadingViewModel(IRoutingService routingService = null, IIdentityService identityService = null, IDialogService dialogService = null)
        {
            _routingService = routingService ?? Locator.Current.GetService<IRoutingService>();
            _identityService = identityService ?? Locator.Current.GetService<IIdentityService>();
            _dialogService = dialogService ?? Locator.Current.GetService<IDialogService>();
        }

        // Called by the views OnAppearing method
        public async void Init()
        {
            try
            {
                var isAuthenticated = _identityService.IsUserLoggedIn;
                if (isAuthenticated)
                {
                    await _routingService.NavigateTo("///main");
                }
                else
                {
                    await _routingService.NavigateTo("///login");
                }
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                await _dialogService.Alert("Error", "An Error Occurred", "OK");
            }
        }
    }
}
