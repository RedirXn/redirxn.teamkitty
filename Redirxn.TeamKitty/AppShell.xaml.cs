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
            Routing.RegisterRoute(nameof(StockItemPage), typeof(StockItemPage));
            Routing.RegisterRoute(nameof(StockPage), typeof(StockPage));
            Routing.RegisterRoute(nameof(MultiTickPage), typeof(MultiTickPage));
            Routing.RegisterRoute(nameof(KittyPage), typeof(KittyPage));
            Routing.RegisterRoute(nameof(StatusPage), typeof(StatusPage));

            BindingContext = this;
        }

        public ICommand ExecuteLogout => new Command(async () => await GoToAsync("main/login"));
    }
}
