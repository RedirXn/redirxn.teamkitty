using System;
using System.Threading.Tasks;

using TeamKitty.Forms.Domain;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TeamKitty.Forms
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class TeamConfigPage : ContentPage
	{
        private readonly TeamService teamService;

        public TeamConfigPage (TeamService teamService)
		{
			InitializeComponent ();
            this.teamService = teamService;
        }

        private async void Save_ClickedAsync(object sender, EventArgs e)
        {
            Team team = teamService.AddTeam(teamName.Text);

            if (isBeer.IsToggled)        
                teamService.AddSku(team, "Beer", decimal.Parse(beerPrice.Text));
            if (isSoftDrink.IsToggled)
                teamService.AddSku(team, "Soft Drink", decimal.Parse(softDrinkPrice.Text));
            if (isPreMix.IsToggled)
                teamService.AddSku(team, "Pre-Mix", decimal.Parse(preMixPrice.Text));

            teamService.AddTeamMember(team, App.LoggedInUser);

            await Exit();
        }

        private async void Cancel_ClickedAsync(object sender, EventArgs e)
        {
            await Exit();
        }

        private async Task Exit()
        {
            await Navigation.PopAsync();
        }
    }
}