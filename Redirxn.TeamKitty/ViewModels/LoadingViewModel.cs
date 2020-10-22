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
        private readonly IRoutingService routingService;
        private readonly IIdentityService identityService;

        public LoadingViewModel(IRoutingService routingService = null, IIdentityService identityService = null)
        {
            this.routingService = routingService ?? Locator.Current.GetService<IRoutingService>();
            this.identityService = identityService ?? Locator.Current.GetService<IIdentityService>();
        }

        // Called by the views OnAppearing method
        public async void Init()
        {
            var isAuthenticated = identityService.IsUserLoggedIn;
            if (isAuthenticated)
            {
                await this.routingService.NavigateTo("///main");
            }
            else
            {
                await this.routingService.NavigateTo("///login");
            }
        }
    }
}
