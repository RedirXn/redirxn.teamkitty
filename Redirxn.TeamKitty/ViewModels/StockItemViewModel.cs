using Redirxn.TeamKitty.Services.Gateway;
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
    public class StockItemViewModel : BaseViewModel
    {
        private IRoutingService _navigationService;
        private IDataStore _dataStore;

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
        public StockItemViewModel(IRoutingService navigationService = null, IDataStore dataStore = null)
        {
            _navigationService = navigationService ?? Locator.Current.GetService<IRoutingService>();
            _dataStore = dataStore ?? Locator.Current.GetService<IDataStore>();
        }
        internal void Save()
        {
            _dataStore.SaveStockItem(_mainName, _mainNamePlural, _stockName, _price, _stockPrice);
        }

    }
}
