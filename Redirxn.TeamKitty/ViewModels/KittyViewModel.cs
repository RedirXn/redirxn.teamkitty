using Redirxn.TeamKitty.Models;
using Redirxn.TeamKitty.Services.Application;
using Redirxn.TeamKitty.Services.Logic;
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
    public partial class KittyViewModel : BaseViewModel
    {
        IKittyService _kittyService;
        IIdentityService _identityService;
        IDialogService _dialogService;
        IRoutingService _routingService;

        public ICommand LoadItemsCommand { get; }
        public Command<LedgerSummaryLine> ItemTapped { get; }
        public ObservableCollection<LedgerSummaryLine> Items { get; }

        private string _currentKitty = string.Empty;
        public string CurrentKitty
        {
            get { return _currentKitty; }
            set { SetProperty(ref _currentKitty, value); }
        }
        private string _kittyBalanceText = string.Empty;
        public string KittyBalanceText
        {
            get { return _kittyBalanceText; }
            set { SetProperty(ref _kittyBalanceText, value); }
        }
        private string _kittyOnHandText = string.Empty;
        public string KittyOnHandText
        {
            get { return _kittyOnHandText; }
            set { SetProperty(ref _kittyOnHandText, value); }
        }
        private LedgerSummaryLine _selectedItem; 
        public LedgerSummaryLine SelectedItem
        {
            get => _selectedItem;
            set
            {
                SetProperty(ref _selectedItem, value);
                OnItemSelected(value);
            }
        }
        public KittyViewModel(IRoutingService navigationService = null, IKittyService kittyService = null, IIdentityService identityService = null, IDialogService dialogService = null)
        {
            _routingService = navigationService ?? Locator.Current.GetService<IRoutingService>();
            _kittyService = kittyService ?? Locator.Current.GetService<IKittyService>();
            _identityService = identityService ?? Locator.Current.GetService<IIdentityService>();
            _dialogService = dialogService ?? Locator.Current.GetService<IDialogService>();

            Items = new ObservableCollection<LedgerSummaryLine>();

            LoadItemsCommand = new Command(async () => await ExecuteLoadItemsCommand());
            ItemTapped = new Command<LedgerSummaryLine>(OnItemSelected);

        }
        async Task ExecuteLoadItemsCommand()
        {
            IsBusy = true;

            try
            {
                Items.Clear();
                foreach (var item in _kittyService.Kitty.Ledger.Summary)
                {
                    Items.Add(item);
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
            KittyBalanceText = "When all money collected: $" + _kittyService.GetKittyBalance();
            KittyOnHandText = " Collected so far: $" + _kittyService.GetKittyOnHand();
        }
        async void OnItemSelected(LedgerSummaryLine item)
        {
            try
            {
                if (item == null)
                    return;
                await _routingService.NavigateTo($"{nameof(StatusPage)}?{nameof(StatusViewModel.FromMember)}={item.Person.Email}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                await _dialogService.Alert("Error", "An Error Occurred", "OK");
            }
        }

    }
}
