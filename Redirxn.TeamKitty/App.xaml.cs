using Xamarin.Forms;
using Splat;
using Redirxn.TeamKitty.ViewModels;
using Redirxn.TeamKitty.Services.Gateway;
using Redirxn.TeamKitty.Services.Application;
using Redirxn.TeamKitty.Services.Logic;

namespace Redirxn.TeamKitty
{
    public partial class App : Application
    {

        public App()
        {
            //Register Syncfusion license
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Mzg4Mzc1QDMxMzgyZTM0MmUzMEZuYVVGMVVCc2NrZVd0MmRWaXM4dGdkaEFFaWRCdk5zOU5LRmdhU2llVGc9");
            
            InitializeComponent();
            InitializeDi();
                        
            MainPage = new AppShell();
        }
        private void InitializeDi()
        {
            // Xamarin Services
            Locator.CurrentMutable.RegisterLazySingleton<IRoutingService>(() => new ShellRoutingService());
            Locator.CurrentMutable.RegisterLazySingleton<IDialogService>(() => new DialogService());

            // Business Services
            Locator.CurrentMutable.RegisterLazySingleton<IIdentityService>(() => new IdentityService());            
            Locator.CurrentMutable.RegisterLazySingleton<IKittyService>(() => new KittyService());
            Locator.CurrentMutable.RegisterLazySingleton<IInviteService>(() => new InviteService());
            Locator.CurrentMutable.RegisterLazySingleton<ICommunicationService>(() => new CommunicationService());

            // DataStores
            var dynamoDataStore = new DynamoDataStore();
            Locator.CurrentMutable.RegisterLazySingleton<IKittyDataStore>(() => dynamoDataStore);
            Locator.CurrentMutable.RegisterLazySingleton<IJoinCodeDataStore>(() => dynamoDataStore);
            Locator.CurrentMutable.RegisterLazySingleton<IUserDataStore>(() => dynamoDataStore);
            Locator.CurrentMutable.RegisterLazySingleton<ICommsDataStore>(() => dynamoDataStore);

            // ViewModels
            Locator.CurrentMutable.Register(() => new LoadingViewModel());
            Locator.CurrentMutable.Register(() => new LoginViewModel());
            Locator.CurrentMutable.Register(() => new MainViewModel());
            Locator.CurrentMutable.Register(() => new MultiTickViewModel());
            Locator.CurrentMutable.Register(() => new SessionViewModel());
            Locator.CurrentMutable.Register(() => new StatusViewModel());
            Locator.CurrentMutable.Register(() => new StockViewModel());
            Locator.CurrentMutable.Register(() => new StockItemViewModel());
            Locator.CurrentMutable.Register(() => new SettingsViewModel());
            Locator.CurrentMutable.Register(() => new KittyViewModel());
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
