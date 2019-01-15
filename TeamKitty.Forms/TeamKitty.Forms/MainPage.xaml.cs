using System;
using System.Linq;
using TeamKitty.Forms.Domain;
using Xamarin.Forms;

namespace TeamKitty.Forms
{
    public partial class MainPage : ContentPage
    {
        private readonly TeamService teamService;

        public MainPage(TeamService teamService)
        {
            InitializeComponent();
            this.teamService = teamService;            
        }

        private void ArrangePage()
        {
            teamButtonsStacker.Children.Clear();
            var teams = teamService.GetTeams();
            foreach (var team in teams)
            {
                Button teamButton = new Button() { Text = team.Name, };
                teamButton.Clicked += TeamButton_ClickedAsync;
                teamButtonsStacker.Children.Add(teamButton);
            }
        }

        private async void TeamButton_ClickedAsync(object sender, EventArgs e)
        {
            var team = teamService.GetTeams().First(t => t.Name == (sender as Button).Text);
            await Navigation.PushAsync(new TeamActivity(team));
        }

        private async void NewTeam_ClickedAsync(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new TeamConfigPage(teamService));            
        }

        private void ContentPage_Appearing(object sender, EventArgs e)
        {
            ArrangePage();
        }
    }
}
