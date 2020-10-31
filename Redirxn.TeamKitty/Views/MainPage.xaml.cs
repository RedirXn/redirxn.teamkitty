using Redirxn.TeamKitty.ViewModels;
using Splat;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Redirxn.TeamKitty.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            this.BindingContext = ViewModel;
        }

        internal MainViewModel ViewModel { get; set; } = Locator.Current.GetService<MainViewModel>();

        protected override void OnAppearing()
        {
            base.OnAppearing();

            ViewModel.Init();            
        }

    }
}