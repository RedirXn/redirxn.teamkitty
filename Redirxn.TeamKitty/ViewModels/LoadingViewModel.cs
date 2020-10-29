using Redirxn.TeamKitty.Services.Application;
using Redirxn.TeamKitty.Services.Logic;
using Splat;

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
