using System;
using System.Collections.Generic;
using System.Windows.Input;
using Redirxn.TeamKitty.ViewModels;
using Redirxn.TeamKitty.Views;
using Xamarin.Forms;

namespace Redirxn.TeamKitty
{
    public partial class AppShell : Xamarin.Forms.Shell
    {
        public AppShell()
        {
            InitializeComponent();
                        
            Routing.RegisterRoute("main/login", typeof(LoginPage));
            BindingContext = this;
        }

        public ICommand ExecuteLogout => new Command(async () => await GoToAsync("main/login"));
    }
}
