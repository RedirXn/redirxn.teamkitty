using Redirxn.TeamKitty.ViewModels;
using Splat;
using System;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Redirxn.TeamKitty.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SettingsPage : ContentPage
    {        
        public SettingsPage()
        {
            InitializeComponent();
            this.BindingContext = ViewModel;
        }
        internal SettingsViewModel ViewModel { get; set; } = Locator.Current.GetService<SettingsViewModel>();

        private async void BtnInvite_Clicked(object sender, EventArgs e)
        {
            await ViewModel.InviteClicked(); // TODO: Replace with command
        }
        private async void BtnAddUser_Clicked(object sender, EventArgs e)
        {
            await ViewModel.AddUserClicked(); // TODO: Replace with command
        }
        private async void BtnJoin_Clicked(object sender, EventArgs e)
        {
            await ViewModel.JoinClicked(); // TODO: Replace with command
        }

    }
}