using Redirxn.TeamKitty.ViewModels;
using Splat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Redirxn.TeamKitty.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class StatusPage : ContentPage
    {
        public StatusPage()
        {
            InitializeComponent();
            this.BindingContext = ViewModel;
        }
        internal StatusViewModel ViewModel { get; set; } = Locator.Current.GetService<StatusViewModel>();
    }
}