using Redirxn.TeamKitty.Services.Identity;
using Redirxn.TeamKitty.Services.Routing;
using Splat;
using System;
using System.Collections.Generic;
using System.Text;

namespace Redirxn.TeamKitty.ViewModels
{
    class LoadingViewModel : BaseViewModel
    {
        private readonly IRoutingService _routingService;
        private readonly IIdentityService _identityService;

        public LoadingViewModel(IRoutingService routingService = null, IIdentityService identityService = null)
        {
            _routingService = routingService ?? Locator.Current.GetService<IRoutingService>();
            _identityService = identityService ?? Locator.Current.GetService<IIdentityService>();
        }

        // Called by the views OnAppearing method
        public async void Init()
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
    }
}
