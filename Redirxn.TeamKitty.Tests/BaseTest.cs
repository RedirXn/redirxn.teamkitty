using NUnit.Framework;
using Redirxn.TeamKitty.Services.Application;
using Redirxn.TeamKitty.Services.Gateway;
using Redirxn.TeamKitty.Services.Logic;
using Splat;

namespace Redirxn.TeamKitty.Tests
{
    public class BaseTest
    {
        protected MockDataStore Db;
        protected MockRoute Routes;
        protected MockDialog Dialogs;

        [SetUp]
        public virtual void Setup()
        {
            Locator.CurrentMutable.RegisterLazySingleton<IIdentityService>(() => new IdentityService());
            Locator.CurrentMutable.RegisterLazySingleton<IKittyService>(() => new KittyService());
            Locator.CurrentMutable.RegisterLazySingleton<IInviteService>(() => new InviteService());

            // Xamarin
            Routes = new MockRoute();
            Locator.CurrentMutable.RegisterLazySingleton<IRoutingService>(() => Routes);
            Dialogs = new MockDialog();
            Locator.CurrentMutable.RegisterLazySingleton<IDialogService>(() => Dialogs);

            // DataStores
            Db = new MockDataStore();
            Locator.CurrentMutable.RegisterLazySingleton<IKittyDataStore>(() => Db);
            Locator.CurrentMutable.RegisterLazySingleton<IJoinCodeDataStore>(() => Db);
            Locator.CurrentMutable.RegisterLazySingleton<IUserDataStore>(() => Db);
        }

    }
}
