using System;
using System.Collections.Generic;
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
            Routing.RegisterRoute(nameof(ItemDetailPage), typeof(ItemDetailPage));
            Routing.RegisterRoute(nameof(NewItemPage), typeof(NewItemPage));
        }

    }
}
