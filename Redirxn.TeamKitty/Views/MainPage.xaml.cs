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
            if (ViewModel.CurrentKitty == "")
            {
                string action = await DisplayActionSheet("You do not belong to a kitty", "Cancel", null, "Create a New Kitty", "Join an Existing Kitty");
                Debug.WriteLine("Action: " + action);
            }
        }

        
    }
}