using Redirxn.TeamKitty.Models;
using Redirxn.TeamKitty.Services.Logic;
using Splat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Redirxn.TeamKitty.ViewModels
{
    public class StatusViewModel : BaseViewModel
    {
        private IKittyService _kittyService;
        private IIdentityService _identityService;

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

        public StatusViewModel(IKittyService kittyService = null, IIdentityService identityService = null)
        {
            _kittyService = kittyService ?? Locator.Current.GetService<IKittyService>();
            _identityService = identityService ?? Locator.Current.GetService<IIdentityService>();

            _myDisplayName = _identityService.UserDetail.Name;
            _myBalanceText = GetBalanceText(_kittyService.Kitty.Ledger.Summary.FirstOrDefault(lsl => lsl.Person.Email == _identityService.LoginData.Email).Balance);
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
