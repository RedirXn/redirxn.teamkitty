using Redirxn.TeamKitty.ViewModels;
using Splat;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Redirxn.TeamKitty.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class KittyPage : ContentPage
    {
        public KittyPage()
        {
            InitializeComponent();
            this.BindingContext = ViewModel;            
        }
        internal KittyViewModel ViewModel { get; set; } = Locator.Current.GetService<KittyViewModel>();
        protected override void OnAppearing()
        {
            base.OnAppearing();
            ViewModel.OnAppearing();
        }

    }
}