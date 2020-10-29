using Redirxn.TeamKitty.Models;
using Redirxn.TeamKitty.Services.Application;
using Redirxn.TeamKitty.Services.Logic;
using Splat;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace Redirxn.TeamKitty.ViewModels
{
    [QueryProperty(nameof(FromMainName), nameof(FromMainName))]
    public class StockItemViewModel : BaseViewModel
    {
        private IRoutingService _navigationService;
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
        public ICommand OnDeleteStockCommand { get; set; }
        public StockItemViewModel(IRoutingService navigationService = null, IKittyService kittyService = null)
        {
            _navigationService = navigationService ?? Locator.Current.GetService<IRoutingService>();            
            _kittyService = kittyService ?? Locator.Current.GetService<IKittyService>();

            OnDeleteStockCommand = new Command(async () => await ExecuteDeleteItemCommand());
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

            await _kittyService.SaveStockItem(stockItem);
            await ClosePage();
        }

        private async Task ClosePage()
        {
            await _navigationService.GoBack();
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

        private async Task ExecuteDeleteItemCommand()
        {
            // ToDO: confirmation, or check not already in use.
            await _kittyService.DeleteStockItem(MainName);
            await ClosePage();
        }

    }
}
