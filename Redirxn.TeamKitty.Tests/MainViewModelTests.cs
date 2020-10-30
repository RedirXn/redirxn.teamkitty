using NUnit.Framework;
using Redirxn.TeamKitty.ViewModels;
using Redirxn.TeamKitty.Services.Application;
using Redirxn.TeamKitty.Services.Gateway;
using Redirxn.TeamKitty.Services.Logic;
using Splat;
using Redirxn.TeamKitty.Models;
using FluentAssertions;

namespace Redirxn.TeamKitty.Tests
{
    [TestFixture]
    public class MainViewModelTests
    {
        MockDataStore mockDb;
        MockRoute mockRoute;
        MockDialog mockDialog;
             
        [SetUp]
        public void Setup()
        {
            Locator.CurrentMutable.RegisterLazySingleton<IIdentityService>(() => new IdentityService());
            Locator.CurrentMutable.RegisterLazySingleton<IKittyService>(() => new KittyService());
            Locator.CurrentMutable.RegisterLazySingleton<IInviteService>(() => new InviteService());

            // Xamarin
            mockRoute = new MockRoute();
            Locator.CurrentMutable.RegisterLazySingleton<IRoutingService>(() => mockRoute);
            mockDialog = new MockDialog();
            Locator.CurrentMutable.RegisterLazySingleton<IDialogService>(() => mockDialog);

            // DataStores
            mockDb = new MockDataStore();
            Locator.CurrentMutable.RegisterLazySingleton<IKittyDataStore>(() => mockDb);
            Locator.CurrentMutable.RegisterLazySingleton<IJoinCodeDataStore>(() => mockDb);
            Locator.CurrentMutable.RegisterLazySingleton<IUserDataStore>(() => mockDb);
        }

        [Test]
        public void KittylessShouldPromptToCreateOrJoin()
        {
            var fakeLoginData = new NetworkAuthData();
            Locator.Current.GetService<IIdentityService>().Init("fakeToken", fakeLoginData);            

            var vmMain = new MainViewModel();           
            vmMain.Init();

            mockDialog.SelectOptionCalled.Should().BeTrue();
        }

        [Test]
        public void KittylessCanCreateANewKitty()
        {
            var fakeLoginData = new NetworkAuthData();
            Locator.Current.GetService<IIdentityService>().Init("fakeToken", fakeLoginData);
            mockDialog.Make_SelectOptionReturn("Create a New Kitty");
            mockDialog.Make_TextInputReturn("My New Kitty");

            var vmMain = new MainViewModel();
            vmMain.Init();

            mockDb.SaveKittyToDbCalled.Should().BeTrue();
        }
    }
}