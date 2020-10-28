﻿using Redirxn.TeamKitty.ViewModels;
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
            string joinCode = await ViewModel.GetKittyJoinCode();

            await DisplayAlert("Join Code", "Advise people to use this code: " + joinCode + " to join your kitty. (expires in 24 hours)", "OK");
        }
        private async void BtnAddUser_Clicked(object sender, EventArgs e)
        {
            string newUser = await DisplayPromptAsync("Add a Non-App User", "Enter the name for the person:");

            if (!string.IsNullOrWhiteSpace(newUser))
            {
                await ViewModel.AddNewUser(newUser);
            }
        }
        private async void BtnJoin_Clicked(object sender, EventArgs e)
        {
            string joinCode =  await DisplayPromptAsync("Join a Kitty", "Enter the code given to you by the Kitty Administrator:");

            if (!string.IsNullOrWhiteSpace(joinCode))
            {
                await ViewModel.JoinKittyWithCode(joinCode);
            }
        }

    }
}