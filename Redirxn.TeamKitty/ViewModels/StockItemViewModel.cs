﻿using Redirxn.TeamKitty.Models;
using Redirxn.TeamKitty.Services.Application;
using Redirxn.TeamKitty.Services.Logic;
using Splat;
using System;
using System.Diagnostics;
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
        private IDialogService _dialogService;

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
        private string _currentKitty = string.Empty;
        public string CurrentKitty
        {
            get { return _currentKitty; }
            set { SetProperty(ref _currentKitty, value); }
        }

        public ICommand OnDeleteStockCommand { get; set; }
        public ICommand SaveItemCommand { get; set; }
        public StockItemViewModel(IRoutingService navigationService = null, IKittyService kittyService = null, IDialogService dialogService = null)
        {
            _navigationService = navigationService ?? Locator.Current.GetService<IRoutingService>();            
            _kittyService = kittyService ?? Locator.Current.GetService<IKittyService>();
            _dialogService = dialogService ?? Locator.Current.GetService<IDialogService>();

            OnDeleteStockCommand = new Command(async () => await ExecuteDeleteItemCommand());
            SaveItemCommand = new Command(async () => await Save());

            CurrentKitty = _kittyService.Kitty?.DisplayName;
        }

        internal async Task Save()
        {
            var stockItem = new StockItem
            {
                MainName = _mainName,
                PluralName = _mainNamePlural,
                StockGrouping = _stockName,
                SalePrice = _price,
            };

            try
            { 
                await _kittyService.SaveStockItem(stockItem);
                await ClosePage();
            }
            catch (ApplicationException ex)
            {
                await _dialogService.Alert("Error", ex.Message, "OK");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                await _dialogService.Alert("Error", "An Error Occurred", "OK");
            }

        }

        private async Task ClosePage()
        {
            await _navigationService.GoBack();
        }

        private async void LoadFromState(string name)
        {
            try
            { 
                var item = _kittyService.Kitty.KittyConfig.StockItems.First(si => si.MainName == name);

                MainName = item.MainName;
                MainNamePlural = item.PluralName;
                StockName = item.StockGrouping;
                Price = item.SalePrice;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                await _dialogService.Alert("Error", "An Error Occurred", "OK");
            }

        }

        private async Task ExecuteDeleteItemCommand()
        {
            try
            {
                if (await _dialogService.Confirm("Delete Stock Item", "Are you sure you want to delete this item?", "Delete", "Cancel"))
                {
                    await _kittyService.DeleteStockItem(MainName);
                    await ClosePage();
                }
            }
            catch (ApplicationException ex)
            {
                await _dialogService.Alert("Error", ex.Message, "OK");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                await _dialogService.Alert("Error", "An Error Occurred", "OK");
            }
        }

    }
}
