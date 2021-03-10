using Redirxn.TeamKitty.Models;
using Redirxn.TeamKitty.Services.Application;
using Redirxn.TeamKitty.Services.Logic;
using Redirxn.TeamKitty.Views;
using Splat;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
        private IDialogService _dialogService;

        private StockDisplay _selectedItem;

        public ICommand OnAddStockCommand { get; set; }        
        public ICommand LoadItemsCommand { get; }
        public ICommand ItemTapped { get; }
        public ObservableCollection<StockDisplay> Items { get; }
        public bool IsAdmin { get; set; } = false;
        private bool _isKittyLocked = false;
        public bool IsKittyLocked
        {
            get => _isKittyLocked;
            set { SetProperty(ref _isKittyLocked, value); }
        }
        private string _currentKitty = string.Empty;
        public string CurrentKitty
        {
            get { return _currentKitty; }
            set { SetProperty(ref _currentKitty, value); }
        }

        public StockViewModel(IRoutingService navigationService = null, IKittyService kittyService = null, IIdentityService identityService = null, IDialogService dialogService = null)
        {
            _navigationService = navigationService ?? Locator.Current.GetService<IRoutingService>();
            _kittyService = kittyService ?? Locator.Current.GetService<IKittyService>();            
            _identityService = identityService ?? Locator.Current.GetService<IIdentityService>();
            _dialogService = dialogService ?? Locator.Current.GetService<IDialogService>();

            Items = new ObservableCollection<StockDisplay>();

            OnAddStockCommand = new Command(async () => await _navigationService.NavigateTo($"{nameof(StockItemPage)}"));            
            LoadItemsCommand = new Command(async () => await ExecuteLoadItemsCommand());
            ItemTapped = new Command<StockDisplay>(async (item) => await OnItemSelected(item));

            IsAdmin = _kittyService.AmIAdmin(_identityService.LoginData.Email);
        }
        async Task ExecuteLoadItemsCommand()
        {
            IsBusy = true;

            try
            {
                Items.Clear();
                foreach (var item in _kittyService.Kitty.KittyConfig.StockItems)
                {
                    var sd = new StockDisplay
                    {
                        MainName = item.MainName,
                        Cost = string.Format("{0:C}", item.SalePrice),
                        Grouping = item.StockGrouping + " of " + item.MainName + " cost about " + string.Format("{0:C}", item.StockPrice),
                    };
                    Items.Add(sd);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                await _dialogService.Alert("Error", "An Error Occurred", "OK");
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
            CurrentKitty = _kittyService.Kitty?.DisplayName;
        }

        public StockDisplay SelectedItem
        {
            get => _selectedItem;
            set
            {
                SetProperty(ref _selectedItem, value);
            }
        }

        async Task OnItemSelected(StockDisplay item)
        {
            try
            { 
                if (item == null || !IsAdmin)
                    return;
                await _navigationService.NavigateTo($"{nameof(StockItemPage)}?{nameof(StockItemViewModel.FromMainName)}={item.MainName}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                await _dialogService.Alert("Error", "An Error Occurred", "OK");
            }
        }
    }

    public class StockDisplay
    {
        public string MainName { get; set; }
        public string Cost { get; set; }
        public string Grouping { get; set; }
    }

}
