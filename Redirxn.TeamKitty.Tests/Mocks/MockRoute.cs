using Redirxn.TeamKitty.Services.Application;
using System;
using System.Threading.Tasks;

namespace Redirxn.TeamKitty.Tests
{
    public class MockRoute : IRoutingService
    {
        private string _navigatedTo;
        private bool _goBack;
        public async Task GoBack()
        {
            _goBack = true;
        }

        public Task GoBackModal()
        {
            throw new System.NotImplementedException();
        }

        public async Task NavigateTo(string route)
        {
            _navigatedTo = route;
        }

        internal bool WasNavigatedTo(string route)
        {
            return _navigatedTo == route;
        }

        internal bool WasGoBackCalled()
        {
            return _goBack;
        }
    }
}