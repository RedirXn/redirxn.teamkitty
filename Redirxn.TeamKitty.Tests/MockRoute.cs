using Redirxn.TeamKitty.Services.Application;
using System;
using System.Threading.Tasks;

namespace Redirxn.TeamKitty.Tests
{
    public class MockRoute : IRoutingService
    {
        private string _navigatedTo;
        public Task GoBack()
        {
            throw new System.NotImplementedException();
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
    }
}