using NUnit.Framework;
using Redirxn.TeamKitty.ViewModels;
using Redirxn.TeamKitty.Services.Logic;
using Splat;
using Redirxn.TeamKitty.Models;
using FluentAssertions;
using Redirxn.TeamKitty.Views;

namespace Redirxn.TeamKitty.Tests
{
    [TestFixture]
    public class MainViewModelTests : BaseTest
    {
        [SetUp]
        public override void Setup()
        {
            base.Setup();

            var fakeLoginData = new NetworkAuthData();
            Locator.Current.GetService<IIdentityService>().Init("fakeToken", fakeLoginData);
        }

        [Test]
        public void KittylessRedirectsToSettingsPage()
        {
            var vmMain = new MainViewModel();

            vmMain.Init();

            Routes.WasNavigatedTo(nameof(SettingsPage)).Should().BeTrue();
        }

    }
}