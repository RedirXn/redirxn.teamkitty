using TeamKitty.Forms.Domain;
using TeamKitty.Forms.Repository;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace TeamKitty.Forms
{
    public partial class App : Application
    {
        public static TeamService teamService;
        public static string LoggedInUser;

        public App()
        {
            InitializeComponent();

            LoggedInUser = "me";
            teamService = new TeamService(new InMemoryRepository());
            MainPage = new NavigationPage(new MainPage(teamService));
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
