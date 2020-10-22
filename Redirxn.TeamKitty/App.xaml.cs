using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Redirxn.TeamKitty.Services;
using Redirxn.TeamKitty.Views;
using Splat;
using Redirxn.TeamKitty.Services.Routing;
using Redirxn.TeamKitty.Services.Identity;
using Redirxn.TeamKitty.ViewModels;
using Redirxn.TeamKitty.Services.Gateway;

namespace Redirxn.TeamKitty
{
    public partial class App : Application
    {

        public App()
        {
            InitializeComponent();
            InitializeDi();
                        
            MainPage = new AppShell();
        }
        private void InitializeDi()
        {
            // Services
            Locator.CurrentMutable.RegisterLazySingleton<IRoutingService>(() => new ShellRoutingService());
            Locator.CurrentMutable.RegisterLazySingleton<IIdentityService>(() => new IdentityService());
            Locator.CurrentMutable.RegisterLazySingleton<IDataStore>(() => new AwsDataStore());

            // ViewModels
            Locator.CurrentMutable.Register(() => new LoadingViewModel());
            Locator.CurrentMutable.Register(() => new LoginViewModel());
            Locator.CurrentMutable.Register(() => new MainViewModel());
        }
        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
