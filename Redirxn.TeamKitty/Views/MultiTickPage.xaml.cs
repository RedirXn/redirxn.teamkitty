using Redirxn.TeamKitty.ViewModels;
using Splat;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Redirxn.TeamKitty.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MultiTickPage : ContentPage
    {
        public MultiTickPage()
        {
            InitializeComponent();
            this.BindingContext = ViewModel;
        }
        internal MultiTickViewModel ViewModel { get; set; } = Locator.Current.GetService<MultiTickViewModel>();
    }
}