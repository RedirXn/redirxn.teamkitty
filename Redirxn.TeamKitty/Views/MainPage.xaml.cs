using Redirxn.TeamKitty.ViewModels;
using Splat;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            await ViewModel.Init();
            if (string.IsNullOrEmpty(ViewModel.CurrentKitty))
            {
                string createKitty = "Create a New Kitty";
                string action = await DisplayActionSheet("You do not belong to a kitty", "Cancel", null, createKitty, "Join an Existing Kitty");
                Debug.WriteLine("Action: " + action);
                if (action == createKitty)
                {
                    string newKittyName = await DisplayPromptAsync("Create New Kitty", "Enter a name for the new kitty:");
                    if (newKittyName != null)
                    {
                        await ViewModel.CreateNewKitty(newKittyName);
                    }
                }
            }
        }
        private void xamlSwitch_Toggled(object sender, ToggledEventArgs e)
        {
            if (e.Value)
            {
                switchStateLabel.Text = $"You are in the session.";
                btnFridge.IsEnabled = true;
            }
            else
            {
                switchStateLabel.Text = $"Join the session.";
                btnFridge.IsEnabled = false;
            }          
            
        }

    }
}