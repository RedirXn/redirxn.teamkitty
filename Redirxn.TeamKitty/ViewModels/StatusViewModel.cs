using Redirxn.TeamKitty.Services.Application;
using Redirxn.TeamKitty.Services.Logic;
using Splat;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace Redirxn.TeamKitty.ViewModels
{
    public class StatusViewModel : BaseViewModel
    {
        private IKittyService _kittyService;
        private IIdentityService _identityService;
        private IDialogService _dialogService;

        string _myDisplayName;
        public string MyDisplayName
        {
            get { return _myDisplayName; }
            set { SetProperty(ref _myDisplayName, value); }
        }

        string _myBalanceText;
        public string MyBalanceText
        {
            get { return _myBalanceText; }
            set { SetProperty(ref _myBalanceText, value); }
        }
        private string _currentKitty = string.Empty;
        public string CurrentKitty
        {
            get { return _currentKitty; }
            set { SetProperty(ref _currentKitty, value); }
        }

        public ObservableCollection<Provision> Provisions { get; }
        public ICommand PayCommand { get; set; }
        public ICommand ProvideCommand { get; set; }
        public ICommand LoadProvisionsCommand { get; set; }
        public StatusViewModel(IKittyService kittyService = null, IIdentityService identityService = null, IDialogService dialogService = null)
        {
            _kittyService = kittyService ?? Locator.Current.GetService<IKittyService>();
            _identityService = identityService ?? Locator.Current.GetService<IIdentityService>();
            _dialogService = dialogService ?? Locator.Current.GetService<IDialogService>();

            PayCommand = new Command(async () => await PaymentRequest());
            ProvideCommand = new Command(async () => await ProvisionRequest());
            LoadProvisionsCommand = new Command(async () => await ExecuteLoadProvisionsCommand());

            Provisions = new ObservableCollection<Provision>();
            MyDisplayName = _identityService.UserDetail.Name;
        }
        public void OnAppearing()
        {
            // This "IsBusy" assignment is what triggers the refresh which in turn calls to load the items.
            IsBusy = true;
            CurrentKitty = _kittyService.Kitty?.DisplayName;
            UpdateScreenText();
        }


        async Task ExecuteLoadProvisionsCommand()
        {
            IsBusy = true;

            try
            {
                Provisions.Clear();

                foreach (var item in _kittyService.Kitty?.Ledger.Summary.FirstOrDefault(lsl => lsl.Person.Email == _identityService.LoginData.Email).Provisions)
                {
                    Provisions.Add(new Provision
                    {
                        Key = item.Key,
                        Value = item.Value
                    });
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

        private void UpdateScreenText()
        {            
            MyBalanceText = GetBalanceText(_kittyService.Kitty.Ledger.Summary.FirstOrDefault(lsl => lsl.Person.Email == _identityService.LoginData.Email).Balance);
        }

        private async Task ProvisionRequest()
        {
            try
            {
                string[] options = _kittyService.Kitty.KittyConfig.StockItems.Select(si => si.StockGrouping + " of " + si.MainName).ToArray();
                string option = await _dialogService.SelectOption("Provide Stock", "Cancel", options);

                var sItem = _kittyService.Kitty.KittyConfig.StockItems.FirstOrDefault(si => si.StockGrouping + " of " + si.MainName == option);

                if (sItem != null)
                {
                    await _kittyService.ProvideStock(_identityService.LoginData.Email, sItem);
                }
                ExecuteLoadProvisionsCommand();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                await _dialogService.Alert("Error", "An Error Occurred", "OK");
            }
        }

        private async Task PaymentRequest()
        {
            try
            { 
                string strAmount = await _dialogService.GetSingleMoneyInput("Payment", "How much are you paying?");
                if (!string.IsNullOrWhiteSpace(strAmount))
                {
                    var amount = decimal.Parse(strAmount);
                    await _kittyService.MakePayment(_identityService.LoginData.Email, amount);
                }
                UpdateScreenText();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                await _dialogService.Alert("Error", "An Error Occurred", "OK");
            }

        }

        private string GetBalanceText(decimal balance)
        {
            if (balance == 0M)
            {
                return "You Owe Nothing";
            }
            else if (balance < 0M)
            {
                return string.Format("You owe {0:C}", Math.Abs(balance));
            }
            else
            {
                return string.Format("You are ahead {0:C}", balance);
            }
        }
    }
    public class Provision
    {
        public string Key { get; set; }
        public int Value { get; set; }
    }

}
