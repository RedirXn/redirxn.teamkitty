using Redirxn.TeamKitty.Models;
using Redirxn.TeamKitty.Services.Gateway;
using Redirxn.TeamKitty.Services.Identity;
using Redirxn.TeamKitty.Services.Routing;
using Redirxn.TeamKitty.Views;
using Splat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace Redirxn.TeamKitty.ViewModels
{
    [QueryProperty(nameof(FromMainName), nameof(FromMainName))]
    public class StockItemViewModel : BaseViewModel
    {
        private IRoutingService _navigationService;
        private IDataStore _dataStore;
        private IKittyService _kittyService;

        string _mainName;
        public string MainName
        {
            get { return _mainName; }
            set { SetProperty(ref _mainName, value); }
        }
        string _mainNamePlural;
        public string MainNamePlural
        {
            get { return _mainNamePlural; }
            set { SetProperty(ref _mainNamePlural, value); }
        }
        string _stockName;      
        public string StockName
        {
            get { return _stockName; }
            set { SetProperty(ref _stockName, value); }
        }
        decimal _price;
        public decimal Price
        {
            get { return _price; }
            set { SetProperty(ref _price, value); }
        }
        decimal _stockPrice;
        public decimal StockPrice
        {
            get { return _stockPrice; }
            set { SetProperty(ref _stockPrice, value); }
        }
        public string FromMainName
        {
            get { return string.Empty; }
            set { LoadFromState(value); }
        }
        public StockItemViewModel(IRoutingService navigationService = null, IDataStore dataStore = null, IKittyService kittyService = null)
        {
            _navigationService = navigationService ?? Locator.Current.GetService<IRoutingService>();
            _dataStore = dataStore ?? Locator.Current.GetService<IDataStore>();
            _kittyService = kittyService ?? Locator.Current.GetService<IKittyService>();
        }
        internal async void Save()
        {
            var stockItem = new StockItem
            {
                MainName = _mainName,
                PluralName = _mainNamePlural,
                StockGrouping = _stockName,
                SalePrice = _price,
                StockPrice = _stockPrice
            };

            _kittyService.Kitty.KittyConfig = await _dataStore.SaveStockItem(_kittyService.Kitty.Id, stockItem);

        }
        private void LoadFromState(string name)
        {
            var item = _kittyService.Kitty.KittyConfig.StockItems.First(si => si.MainName == name);

            MainName = item.MainName;
            MainNamePlural = item.PluralName;
            StockName = item.StockGrouping;
            Price = item.SalePrice;
            StockPrice = item.StockPrice;
        }

    }
}
