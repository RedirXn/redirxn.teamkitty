using Redirxn.TeamKitty.ViewModels;
using Splat;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Redirxn.TeamKitty.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class HistoryPage : ContentPage
    {
        public HistoryPage()
        {
            InitializeComponent();
            this.BindingContext = ViewModel;
        }
        internal HistoryViewModel ViewModel { get; set; } = Locator.Current.GetService<HistoryViewModel>();
        protected override void OnAppearing()
        {
            base.OnAppearing();
            ViewModel.OnAppearing();
        }

    }
}