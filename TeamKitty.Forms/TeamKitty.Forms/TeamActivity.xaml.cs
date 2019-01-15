using DevExpress.Mobile.Core.Containers;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TeamKitty.Forms
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class TeamActivity : ContentPage
	{
        private readonly Domain.Team _team;

        public TeamActivity (Domain.Team team)
		{
			InitializeComponent();

            _team = team;

            ArrangePage();

            BindingContext = _team;
            
            
        }

        private void ArrangePage()
        {
            mainLabel.Text = _team.Name;
            foreach (var product in _team.GetSkus())
            {
                Button productButton = new Button() { Text = product.Product.ToString() };
                productButton.Clicked += ProductButton_ClickedAsync;
                ticksStacker.Children.Add(productButton);
            }
            
        }

        private async void ProductButton_ClickedAsync(object sender, EventArgs e)
        {
            var product = (sender as Button).Text;
            if (await DisplayAlert("Purchase", "Add one "+ product + "?", "Yes", "Cancel"))
            {
                App.teamService.PurchaseProduct(_team, App.LoggedInUser, product);
            }
        }

        private void Invite_Clicked(object sender, System.EventArgs e)
        {
            
        }

    }
}