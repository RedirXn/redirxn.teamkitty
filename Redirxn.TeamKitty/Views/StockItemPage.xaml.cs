using Redirxn.TeamKitty.ViewModels;
using Splat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Redirxn.TeamKitty.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class StockItemPage : ContentPage
    {
        public StockItemPage()
        {
            InitializeComponent();
            this.BindingContext = ViewModel;
        }
        internal StockItemViewModel ViewModel { get; set; } = Locator.Current.GetService<StockItemViewModel>();


        protected override bool OnBackButtonPressed()
        {
            return true;
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            // TODO: COnvert to Command
            ViewModel.Save();
        }

        private void Generic_TextChanged(object sender, TextChangedEventArgs e)
        {
            // TODO: Should be bindings
            UpdateHelperTexts();
        }
        private void UpdateHelperTexts()
        {
            lblMainName.Text = $"eg. Get me a {MainName.Text ?? MainName.Placeholder} please.";
            lblMainNamePlural.Text = $"eg. You have consumed 12 {MainNamePlural.Text ?? MainNamePlural.Placeholder}.";
            lblStockName.Text = $"eg. Here is one {StockName.Text ?? StockName.Placeholder} of {MainNamePlural.Text ?? MainNamePlural.Placeholder}.";
            lblPrice.Text = $"eg. One {MainName.Text ?? MainName.Placeholder} costs ${Price.Text ?? Price.Placeholder}";
            lblStockPrice.Text = $"eg. One {StockName.Text ?? StockName.Placeholder} of {MainNamePlural.Text ?? MainNamePlural.Placeholder} usually costs ${StockPrice.Text ?? StockPrice.Placeholder}";
        }
    }
}