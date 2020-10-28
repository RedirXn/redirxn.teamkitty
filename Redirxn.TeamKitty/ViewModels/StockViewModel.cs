using Redirxn.TeamKitty.Models;
using Redirxn.TeamKitty.Services.Gateway;
using Redirxn.TeamKitty.Services.Identity;
using Redirxn.TeamKitty.Services.Routing;
using Redirxn.TeamKitty.Views;
using Splat;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
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
        public ObservableCollection<StockItem> Items { get; }
        public Command LoadItemsCommand { get; }
        public Command<StockItem> ItemTapped { get; }
        public bool IsAdmin { get; set; } = false;

        public StockViewModel(IRoutingService navigationService = null, IKittyService kittyService = null, IIdentityService identityService = null)
        {
            _navigationService = navigationService ?? Locator.Current.GetService<IRoutingService>();
            _kittyService = kittyService ?? Locator.Current.GetService<IKittyService>();            
            _identityService = identityService ?? Locator.Current.GetService<IIdentityService>();

            Items = new ObservableCollection<StockItem>();

            OnAddStockCommand = new Command(async () => await _navigationService.NavigateTo($"{nameof(StockItemPage)}"));            
            LoadItemsCommand = new Command(async () => await ExecuteLoadItemsCommand());
            ItemTapped = new Command<StockItem>(OnItemSelected);

            IsAdmin = _kittyService.AmIAdmin(_identityService.LoginData.Email);
        }
        async Task ExecuteLoadItemsCommand()
        {
            IsBusy = true;

            try
            {
                Items.Clear();
                var kitty = await _kittyService.LoadKitty(_identityService.UserDetail.DefaultKitty);

                foreach (var item in kitty.KittyConfig.StockItems)
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
