using System.ComponentModel;
using Xamarin.Forms;
using Redirxn.TeamKitty.ViewModels;

namespace Redirxn.TeamKitty.Views
{
    public partial class ItemDetailPage : ContentPage
    {
        public ItemDetailPage()
        {
            InitializeComponent();
            BindingContext = new ItemDetailViewModel();
        }
    }
}