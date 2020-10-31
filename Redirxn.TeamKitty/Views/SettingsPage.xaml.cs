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
        protected override async void OnAppearing()
        {
            base.OnAppearing();

            await ViewModel.Init();
        }


    }
}