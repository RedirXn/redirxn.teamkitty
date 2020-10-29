using Redirxn.TeamKitty.Models;
using Redirxn.TeamKitty.Services.Application;
using Redirxn.TeamKitty.Services.Logic;
using Redirxn.TeamKitty.Views;
using Splat;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using Xamarin.Forms;

namespace Redirxn.TeamKitty.ViewModels
{
    public class StockViewModel : BaseViewModel
    {
        private IRoutingService _navigationService;
        private IKittyService _kittyService;
        private IIdentityService _identityService;

        private StockItem _selectedItem;

        public ICommand OnAddStockCommand { get; set; }        
        public ICommand LoadItemsCommand { get; }
        public Command<StockItem> ItemTapped { get; }
        public ObservableCollection<StockItem> Items { get; }
        public bool IsAdmin { get; set; } = false;

        public StockViewModel(IRoutingService navigationService = null, IKittyService kittyService = null, IIdentityService identityService = null)
        {
            _navigationService = navigationService ?? Locator.Current.GetService<IRoutingService>();
            _kittyService = kittyService ?? Locator.Current.GetService<IKittyService>();            
            _identityService = identityService ?? Locator.Current.GetService<IIdentityService>();

            Items = new ObservableCollection<StockItem>();

            OnAddStockCommand = new Command(async () => await _navigationService.NavigateTo($"{nameof(StockItemPage)}"));            
            LoadItemsCommand = new Command(() => ExecuteLoadItemsCommand());
            ItemTapped = new Command<StockItem>(OnItemSelected);

            IsAdmin = _kittyService.AmIAdmin(_identityService.LoginData.Email);
        }
        void ExecuteLoadItemsCommand()
        {
            IsBusy = true;

            try
            {
                Items.Clear();
                foreach (var item in _kittyService.Kitty.KittyConfig.StockItems)
                {
                    Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        public void OnAppearing()
        {
            IsBusy = true;
            SelectedItem = null;
        }

        public StockItem SelectedItem
        {
            get => _selectedItem;
            set
            {
                SetProperty(ref _selectedItem, value);
                OnItemSelected(value);
            }
        }

        async void OnItemSelected(StockItem item)
        {
            if (item == null || !IsAdmin)
                return;
            await _navigationService.NavigateTo($"{nameof(StockItemPage)}?{nameof(StockItemViewModel.FromMainName)}={item.MainName}");
        }
    }
}
