using Redirxn.TeamKitty.Services.Application;
using System.Threading.Tasks;

namespace Redirxn.TeamKitty.Tests
{
    internal class MockRoute : IRoutingService
    {
        public Task GoBack()
        {
            throw new System.NotImplementedException();
        }

        public Task GoBackModal()
        {
            throw new System.NotImplementedException();
        }

        public Task NavigateTo(string route)
        {
            throw new System.NotImplementedException();
        }
    }
}