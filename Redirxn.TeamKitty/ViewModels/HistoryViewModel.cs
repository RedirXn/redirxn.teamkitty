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
    class HistoryViewModel : BaseViewModel
    {
        private IKittyService _kittyService;
        private IIdentityService _identityService;
        private IDialogService _dialogService;
        private IEnumerable<Transaction> _transactions;

        public List<DatedGroupedTransaction> Transactions { get; }
        public ICommand LoadTransactionsCommand { get; set; }

        private string _currentKitty = string.Empty;
        public string CurrentKitty
        {
            get { return _currentKitty; }
            set { SetProperty(ref _currentKitty, value); }
        }
        private bool _noKitty;
        public bool NoKitty
        {
            get { return _noKitty; }
            set { SetProperty(ref _noKitty, value); }
        }
        public bool HasKitty
        {
            get { return !_noKitty; }
        }

        public HistoryViewModel(IKittyService kittyService = null, IIdentityService identityService = null, IDialogService dialogService = null)
        {
            _kittyService = kittyService ?? Locator.Current.GetService<IKittyService>();
            _identityService = identityService ?? Locator.Current.GetService<IIdentityService>();
            _dialogService = dialogService ?? Locator.Current.GetService<IDialogService>();

            LoadTransactionsCommand = new Command(async () => await ExecuteLoadTransactionsCommand());
            
            Transactions = new List<DatedGroupedTransaction>();
        }

        public void OnAppearing()
        {
            CurrentKitty = _kittyService.Kitty?.DisplayName;
            NoKitty = _kittyService.Kitty == null;
            IsBusy = true;
        }
        async Task ExecuteLoadTransactionsCommand()
        {
            IsBusy = true;

            try
            {
                if (_kittyService.Kitty != null)
                {
                    _transactions = _kittyService.Kitty.Ledger.Transactions.OrderByDescending(t => t.Date.Date).ThenBy(t => t.Person.DisplayName);
                    Transactions.Clear();

                    var w = new WipTransaction();
                    var dgt = new DatedGroupedTransaction("", new List<GroupedTransaction>());
                    foreach (var item in _transactions)
                    {
                        var date = item.Date.ToString("ddd, MMM d, yyyy");
                        if (date != w.Date && !string.IsNullOrEmpty(w.Date))
                        {
                            dgt.Add(w.ToGroupedTran());
                            Transactions.Add(dgt);
                            w = new WipTransaction();
                            dgt = new DatedGroupedTransaction(date, new List<GroupedTransaction>());
                        }
                        else
                        {
                            var person = item.Person.DisplayName;
                            if (person != w.Person && !string.IsNullOrEmpty(w.Person))
                            {
                                dgt.Add(w.ToGroupedTran());
                                w = new WipTransaction();
                            }
                            if (string.IsNullOrEmpty(dgt.Name))
                            {
                                dgt.Name = date;
                            }
                        }
                        w.UpdateFromTransaction(date, item);
                    }
                    if (!string.IsNullOrEmpty(w.Date))
                    {
                        dgt.Add(w.ToGroupedTran());
                        Transactions.Add(dgt);
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

    }
}
