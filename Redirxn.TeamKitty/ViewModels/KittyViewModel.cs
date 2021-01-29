using Redirxn.TeamKitty.Models;
using Redirxn.TeamKitty.Services.Application;
using Redirxn.TeamKitty.Services.Logic;
using Redirxn.TeamKitty.Views;
using Splat;
using Syncfusion.SfChart.XForms;
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
        public ICommand AdjustCommand { get; set; }
        public Command<LedgerSummaryLine> ItemTapped { get; }
        public ObservableCollection<LedgerSummaryLine> Items { get; }

        private bool _isAdmin = false;
        public bool IsAdmin
        {
            get => _isAdmin;
            set { SetProperty(ref _isAdmin, value); }
        }
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
        public ObservableCollection<ChartDataPoint> KittyMoney { get; set; }
        public KittyViewModel(IRoutingService navigationService = null, IKittyService kittyService = null, IIdentityService identityService = null, IDialogService dialogService = null)
        {
            _routingService = navigationService ?? Locator.Current.GetService<IRoutingService>();
            _kittyService = kittyService ?? Locator.Current.GetService<IKittyService>();
            _identityService = identityService ?? Locator.Current.GetService<IIdentityService>();
            _dialogService = dialogService ?? Locator.Current.GetService<IDialogService>();

            Items = new ObservableCollection<LedgerSummaryLine>();

            LoadItemsCommand = new Command(async () => await ExecuteLoadItemsCommand());
            ItemTapped = new Command<LedgerSummaryLine>(OnItemSelected);
            AdjustCommand = new Command(async () => await AdjustmentRequest());

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
            IsAdmin = _kittyService.AmIAdmin(_identityService.LoginData.Email);
            CurrentKitty = _kittyService.Kitty?.DisplayName;
            var balance = _kittyService.GetKittyBalance();
            var onHand = _kittyService.GetKittyOnHand();
            KittyBalanceText = "When all money collected: $" + balance;
            KittyOnHandText = " Collected so far: $" + onHand;
            KittyMoney = new ObservableCollection<ChartDataPoint>
            {
                new ChartDataPoint("Received", double.Parse(onHand)),
                new ChartDataPoint("Remaining", double.Parse(balance)-double.Parse(onHand))
            };
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
        private async Task AdjustmentRequest()
        {
            try
            {
                string strAmount = await _dialogService.GetSingleMoneyInput("Adjustment", "How much do you wish to adjust the balance by?");
                if (!string.IsNullOrWhiteSpace(strAmount))
                {
                    var amount = decimal.Parse(strAmount);
                    await _kittyService.AdjustBalanceBy(_identityService.LoginData.Email, amount);
                    KittyBalanceText = "When all money collected: $" + _kittyService.GetKittyBalance();
                    KittyOnHandText = " Collected so far: $" + _kittyService.GetKittyOnHand();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                await _dialogService.Alert("Error", "An Error Occurred", "OK");
            }

        }


    }
}
