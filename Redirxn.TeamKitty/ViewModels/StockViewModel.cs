using Redirxn.TeamKitty.Services.Routing;
using Redirxn.TeamKitty.Views;
using Splat;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace Redirxn.TeamKitty.ViewModels
{
    public class StockViewModel : BaseViewModel
    {
        private IRoutingService _navigationService;
        public ICommand OnAddStockCommand { get; set; }
        
        public StockViewModel(IRoutingService navigationService = null)
        {
            _navigationService = navigationService ?? Locator.Current.GetService<IRoutingService>();

            OnAddStockCommand = new Command(async () => await _navigationService.NavigateTo($"{nameof(StockItemPage)}"));
        }

    }
}
