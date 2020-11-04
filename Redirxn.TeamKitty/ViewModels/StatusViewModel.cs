using Redirxn.TeamKitty.Models;
using Redirxn.TeamKitty.Services.Application;
using Redirxn.TeamKitty.Services.Logic;
using Splat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        public ICommand PayCommand { get; set; }
        public ICommand ProvideCommand { get; set; }
        public StatusViewModel(IKittyService kittyService = null, IIdentityService identityService = null, IDialogService dialogService = null)
        {
            _kittyService = kittyService ?? Locator.Current.GetService<IKittyService>();
            _identityService = identityService ?? Locator.Current.GetService<IIdentityService>();
            _dialogService = dialogService ?? Locator.Current.GetService<IDialogService>();

            PayCommand = new Command(async () => await PaymentRequest());
            ProvideCommand = new Command(async () => await ProvisionRequest());

            _myDisplayName = _identityService.UserDetail.Name;
            _myBalanceText = GetBalanceText(_kittyService.Kitty.Ledger.Summary.FirstOrDefault(lsl => lsl.Person.Email == _identityService.LoginData.Email).Balance);
        }

        private async Task ProvisionRequest()
        {
            string[] options = _kittyService.Kitty.KittyConfig.StockItems.Select(si => si.StockGrouping + " of " + si.MainName).ToArray();
            string option = await _dialogService.SelectOption("Provide Stock", "Cancel");

            var sItem = _kittyService.Kitty.KittyConfig.StockItems.FirstOrDefault(si => si.StockGrouping + " of " + si.MainName == option);

            if (sItem != null)
            {
                await _kittyService.ProvideStock(_identityService.LoginData.Email, sItem);
            }
        }

        private async Task PaymentRequest()
        {
            string strAmount = await _dialogService.GetSingleMoneyInput("Payment", "How much are you paying?");
            if (!string.IsNullOrWhiteSpace(strAmount))
            {
                var amount = decimal.Parse(strAmount);
                await _kittyService.MakePayment(_identityService.LoginData.Email, amount);
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
}
