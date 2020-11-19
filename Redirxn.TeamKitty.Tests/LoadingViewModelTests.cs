using NUnit.Framework;
using Redirxn.TeamKitty.ViewModels;
using Redirxn.TeamKitty.Services.Logic;
using Splat;
using Redirxn.TeamKitty.Models;
using FluentAssertions;
using System.Threading.Tasks;
using System;
using Redirxn.TeamKitty.Services.Application;

namespace Redirxn.TeamKitty.Tests
{
    [TestFixture]
    public class LoadingViewModelTests : BaseTest
    {
        [TestCase(false, "///login")]
        [TestCase(true, "///main")]
        public void WillBranchBasedOnLogin(bool loggedIn, string route)
        {
            var idService = new mockIdentityService { IsUserLoggedIn = loggedIn };
            var vmLoading = new LoadingViewModel(Locator.Current.GetService<IRoutingService>(), idService);

            vmLoading.Init();

            Routes.WasNavigatedTo(route).Should().BeTrue();
        }
        internal class mockIdentityService : IIdentityService
        {
            public bool IsUserLoggedIn { get; set; }

            public NetworkAuthData LoginData { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public UserInfo UserDetail { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public Task AddMeToKitty(string kittyId)
            {
                throw new NotImplementedException();
            }

            public Task Init(string activeToken, NetworkAuthData socialLoginData)
            {
                throw new NotImplementedException();
            }

            public bool KittyNameExists(string newKittyName)
            {
                throw new NotImplementedException();
            }

            public Task Rename(string newName)
            {
                throw new NotImplementedException();
            }

            public Task SetDefaultKitty(string nextKitty)
            {
                throw new NotImplementedException();
            }
        }
    }

}