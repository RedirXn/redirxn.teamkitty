using Redirxn.TeamKitty.Models;
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
    [QueryProperty(nameof(FromMember), nameof(FromMember))]
    public class StatusViewModel : BaseViewModel
    {
        private IKittyService _kittyService;
        private IIdentityService _identityService;
        private IDialogService _dialogService;

        public ICommand PayCommand { get; set; }
        public ICommand ProvideCommand { get; set; }
        public ICommand LoadTransactionsCommand { get; set; }
        public ICommand ChangeMyNameCommand { get; set; }

        private LedgerSummaryLine _summary;
        private IEnumerable<Transaction> _transactions;
        private bool _loadingFromState;

        private bool _isAdmin = false;
        public bool IsAdmin
        {
            get => _isAdmin;
            set { SetProperty(ref _isAdmin, value); }
        }
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
        string _myPaidText;
        public string MyPaidText
        {
            get { return _myPaidText; }
            set { SetProperty(ref _myPaidText, value); }
        }
        string _myProvisionText;
        public string MyProvisionText
        {
            get { return _myProvisionText; }
            set { SetProperty(ref _myProvisionText, value); }
        }
        private string _currentKitty = string.Empty;
        public string CurrentKitty
        {
            get { return _currentKitty; }
            set { SetProperty(ref _currentKitty, value); }
        }
        private bool _isChangeable;
        public bool IsChangeable
        {
            get { return _isChangeable; }
            set { SetProperty(ref _isChangeable, value); }
        }

        public ObservableCollection<GroupedTransaction> Transactions { get; }

        public string FromMember
        {
            get { return string.Empty; }
            set { LoadFromState(value); }
        }

        public StatusViewModel(IKittyService kittyService = null, IIdentityService identityService = null, IDialogService dialogService = null)
        {
            _kittyService = kittyService ?? Locator.Current.GetService<IKittyService>();
            _identityService = identityService ?? Locator.Current.GetService<IIdentityService>();
            _dialogService = dialogService ?? Locator.Current.GetService<IDialogService>();

            PayCommand = new Command(async () => await PaymentRequest());
            ProvideCommand = new Command(async () => await ProvisionRequest());
            LoadTransactionsCommand = new Command(async () => await ExecuteLoadTransactionsCommand());
            ChangeMyNameCommand = new Command(async () => await ChangeMyName());

            Transactions = new ObservableCollection<GroupedTransaction>();           
        }
        public void OnAppearing()
        {            
            CurrentKitty = _kittyService.Kitty?.DisplayName;

            if (!_loadingFromState)
            {
                ReloadSummary(_identityService.LoginData.Email);
            }            
            IsChangeable = IsAdmin || _summary.Person.Email == _identityService.LoginData.Email;
            MyDisplayName = _summary.Person.DisplayName;
            IsAdmin = _kittyService.AmIAdmin(_identityService.LoginData.Email);
            IsChangeable = IsAdmin || _summary.Person.Email == _identityService.LoginData.Email;
            UpdateScreenText();
            _loadingFromState = false;
            IsBusy = true;
        }

        private async void ReloadSummary(string email)
        {
            _summary = _kittyService.Kitty.Ledger.Summary.FirstOrDefault(lsl => lsl.Person.Email == email);
            _transactions = _kittyService.Kitty.Ledger.Transactions.Where(t => t.Person.Email == email).OrderByDescending(t => t.Date);
            await ExecuteLoadTransactionsCommand();
        }

        async Task ExecuteLoadTransactionsCommand()
        {
            IsBusy = true;

            try
            {
                Transactions.Clear();

                if (_summary != null)
                {
                    var w = new WipTransaction();
                    foreach (var item in _transactions)
                    {
                        var date = item.Date.ToString("ddd, MMM d, yyyy");
                        if (date != w.Date && !string.IsNullOrEmpty(w.Date))
                        {
                            Transactions.Add(GroupedTranFromWip(w));
                            w = new WipTransaction();
                        }                        
                        w.UpdateFromTransaction(date, item);
                    }
                    if (!string.IsNullOrEmpty(w.Date))
                    {
                        Transactions.Add(GroupedTranFromWip(w));
                    }
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


        private GroupedTransaction GroupedTranFromWip(WipTransaction w)
        {
            var payments = (w.Payments > 0M) ? "Paid: " + string.Format("{0:C}", w.Payments) : string.Empty;
            var purchases = (w.Purchases.Count > 0) ? "Purchased: " + string.Join(", ", w.Purchases.Select(kv => kv.Value + " " + kv.Key).ToArray()) : string.Empty;
            var provisions = (w.Provisions.Count > 0) ? "Supplied: " + string.Join(", ", w.Provisions.Select(kv => kv.Value + " " + kv.Key).ToArray()) : string.Empty;
            var carried = (w.Carries.Count > 0) ? "Carried Over: " + string.Join(", ", w.Carries.Select(kv => string.Format("{0:C}", kv.Value) + " " +  kv.Key).ToArray()) : string.Empty;

            string summary = purchases + ((!string.IsNullOrEmpty(purchases) && !string.IsNullOrEmpty(provisions)) ? Environment.NewLine : string.Empty) +
                provisions + ((!string.IsNullOrEmpty(purchases + provisions) && !string.IsNullOrEmpty(payments)) ? Environment.NewLine : string.Empty) +
                payments + ((!string.IsNullOrEmpty(purchases + provisions + payments) && !string.IsNullOrEmpty(carried)) ? Environment.NewLine : string.Empty) +
                carried;

            return new GroupedTransaction()
            {
                Date = w.Date,
                DayTotal = (w.Total != 0M) ? string.Format("{0:C}", w.Total) : string.Empty,
                Summary = summary
            };
        }

        private void UpdateScreenText()
        {
            MyBalanceText = GetBalanceText(_summary.Balance);
            MyPaidText = string.Format("Paid so far: {0:C}", Math.Abs(_summary.TotalPaid));
            MyProvisionText = "Supplied: " + _summary.ProvisionText;
        }

        private async Task ProvisionRequest()
        {
            try
            {
                string[] options = _kittyService.Kitty.KittyConfig.StockItems.Select(si => si.StockGrouping + " of " + si.MainName).ToArray();
                string option = await _dialogService.SelectOption("Provide Stock", "Cancel", options);

                if (option != "Cancel")
                {
                    var sItem = _kittyService.Kitty.KittyConfig.StockItems.FirstOrDefault(si => si.StockGrouping + " of " + si.MainName == option);

                    if (sItem != null)
                    {
                        await _kittyService.ProvideStock(_summary.Person.Email, sItem);
                    }
                    ReloadSummary(_summary.Person.Email);
                    UpdateScreenText();
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

        private async Task PaymentRequest()
        {
            try
            { 
                string strAmount = await _dialogService.GetSingleMoneyInput("Payment", "How much are you paying?");
                if (!string.IsNullOrWhiteSpace(strAmount))
                {
                    var amount = decimal.Parse(strAmount);
                    await _kittyService.MakePayment(_summary.Person.Email, amount);
                }
                ReloadSummary(_summary.Person.Email);
                UpdateScreenText();
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
        private string GetBalanceText(decimal balance) 
        {
            if (balance == 0M)
            {
                return "All paid up";
            }
            else if (balance < 0M)
            {
                return string.Format("You still owe {0:C}", Math.Abs(balance));
            }
            else
            {
                return string.Format("You are ahead {0:C}", balance);
            }
        }

        private async void LoadFromState(string email)
        {
            try
            {
                _loadingFromState = true;
                var decodedEmail = System.Web.HttpUtility.UrlDecode(email);
                var item = _kittyService.Kitty.Ledger.Summary.FirstOrDefault(lsl => lsl.Person.Email == decodedEmail);

                MyDisplayName = item.Person.DisplayName;
                _summary = item;
                _transactions = _kittyService.Kitty.Ledger.Transactions.Where(t => t.Person.Email == _summary.Person.Email).OrderByDescending(t => t.Date);
                UpdateScreenText();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                await _dialogService.Alert("Error", "An Error Occurred", "OK");
            }

        }
        internal async Task ChangeMyName()
        {
            try
            {
                string newName = await _dialogService.GetSingleTextInput("Change My Name", "Enter the new name:");

                if (!string.IsNullOrWhiteSpace(newName) && newName != "Cancel")
                {
                    await ChangeMyNameTo(newName);
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

        private async Task ChangeMyNameTo(string newName)
        {
            if (_kittyService.Kitty.Ledger.Summary.FirstOrDefault(lsl => lsl.Person.DisplayName == newName) == null)
            {
                await _kittyService.RenameMember(_identityService.UserDetail.Id, newName);
                await _identityService.Rename(newName);
            }
        }

        public class WipTransaction
        {
            public Dictionary<string, int> Purchases = new Dictionary<string, int>();
            public Dictionary<string, int> Provisions = new Dictionary<string, int>();
            public Dictionary<string, decimal> Carries = new Dictionary<string, decimal>();
            public decimal Payments = 0M;
            public decimal Total = 0M;
            public string Date = string.Empty;

            public void UpdateFromTransaction(string date, Transaction item)
            {                
                switch (item.TransactionType)
                {
                    case TransactionType.CarryOver:
                        {
                            if (Carries.ContainsKey(item.TransactionName))
                            {
                                Carries[item.TransactionName] += item.TransactionAmount;
                            }
                            else
                            {
                                Carries[item.TransactionName] = item.TransactionAmount;
                            }
                            Date = date;
                            Payments += item.TransactionAmount;
                            Total += item.TransactionAmount;
                            break;
                        }
                    case TransactionType.Payment:
                        {                            
                            Date = date;
                            Payments += item.TransactionAmount;
                            Total += item.TransactionAmount;
                            break;
                        }
                    case TransactionType.Purchase:                    
                        {
                            if (Purchases.ContainsKey(item.TransactionName))
                            {
                                Purchases[item.TransactionName] += item.TransactionCount;
                            }
                            else
                            {
                                Purchases[item.TransactionName] = item.TransactionCount;
                            }
                            Date = date;
                            Total -= item.TransactionAmount;
                            break;
                        }
                    case TransactionType.Provision:
                        {
                            if (Provisions.ContainsKey(item.TransactionName))
                            {
                                Provisions[item.TransactionName] += item.TransactionCount;
                            }
                            else
                            {
                                Provisions[item.TransactionName] = item.TransactionCount;
                            }
                            Date = date;
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }
            }

        }
    }

    public class GroupedTransaction
    {
        public string Date { get; set; }        
        public string Summary { get; set; }
        public string DayTotal { get; set; }
    }
}
