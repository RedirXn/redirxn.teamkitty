﻿using Redirxn.TeamKitty.Models;
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
        public ICommand PayCommand { get; set; }
        public ICommand ProvideCommand { get; set; }
        public ICommand LoadTransactionsCommand { get; set; }

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

            Transactions = new ObservableCollection<GroupedTransaction>();            
        }
        public void OnAppearing()
        {            
            CurrentKitty = _kittyService.Kitty?.DisplayName;

            if (!_loadingFromState)
            {
                ReloadSummary(_identityService.LoginData.Email);
            }

            IsAdmin = _kittyService.AmIAdmin(_identityService.LoginData.Email);
            IsChangeable = IsAdmin || _summary.Person.Email == _identityService.LoginData.Email;
            MyDisplayName = _summary.Person.DisplayName;
            UpdateScreenText();
            _loadingFromState = false;
            IsBusy = true;
        }

        private async void ReloadSummary(string email)
        {
            _summary = _kittyService.Kitty.Ledger.Summary.FirstOrDefault(lsl => lsl.Person.Email == email);
            _transactions = _kittyService.Kitty.Ledger.Transactions.Where(t => t.Person.Email == email).OrderBy(t => t.Date);
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
            return new GroupedTransaction()
            {
                Date = w.Date,
                DayTotal = (w.Total != 0M) ? string.Format("{0:C}", w.Total) : string.Empty,
                Payments = (w.Payments > 0M) ? string.Format("{0:C}", w.Payments) : string.Empty,
                Purchases = string.Join(",", w.Purchases.Select(kv => kv.Value + " " + kv.Key).ToArray()),
                Provisions = string.Join(",", w.Provisions.Select(kv => kv.Value + " " + kv.Key).ToArray())
            };
        }

        private void UpdateScreenText()
        {
            MyBalanceText = GetBalanceText(_summary.Balance);
            MyPaidText = string.Format("Paid so far: {0:C}", Math.Abs(_summary.TotalPaid));
            MyProvisionText = "Provided: " + _summary.ProvisionText;
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
                }
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
                _transactions = _kittyService.Kitty.Ledger.Transactions.Where(t => t.Person.Email == _summary.Person.Email).OrderBy(t => t.Date);
                UpdateScreenText();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                await _dialogService.Alert("Error", "An Error Occurred", "OK");
            }

        }
        public class WipTransaction
        {
            public Dictionary<string, int> Purchases = new Dictionary<string, int>();
            public Dictionary<string, int> Provisions = new Dictionary<string, int>();
            public decimal Payments = 0M;
            public decimal Total = 0M;
            public string Date = string.Empty;

            public void UpdateFromTransaction(string date, Transaction item)
            {
                Date = date;
                switch (item.TransactionType)
                {
                    case TransactionType.Payment:
                        {
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
        public string Purchases { get; set; }        
        public string Provisions { get; set; }
        public string Payments { get; set; }
        public string DayTotal { get; set; }
    }
}
