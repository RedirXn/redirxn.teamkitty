using Redirxn.TeamKitty.Services.Application;
using Redirxn.TeamKitty.Services.Logic;
using Splat;
using System.Threading.Tasks;

namespace Redirxn.TeamKitty.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        IKittyService _kittyService;
        IIdentityService _identityService;
        IInviteService _inviteService;
        IDialogService _dialogService;

        public bool IsAdmin { get; set; } = false;

        public SettingsViewModel(IKittyService kittyService = null, IIdentityService identityService = null, IInviteService inviteService = null, IDialogService dialogService = null)
        {
            _kittyService = kittyService ?? Locator.Current.GetService<IKittyService>();
            _identityService = identityService ?? Locator.Current.GetService<IIdentityService>();
            _inviteService = inviteService ?? Locator.Current.GetService<IInviteService>();
            _dialogService = dialogService ?? Locator.Current.GetService<IDialogService>();

            IsAdmin = _kittyService.AmIAdmin(_identityService.LoginData.Email);
        }

        internal async Task JoinClicked()
        {
            string joinCode = await _dialogService.GetSingleTextInput("Join a Kitty", "Enter the code given to you by the Kitty Administrator:");

            if (!string.IsNullOrWhiteSpace(joinCode))
            {
                await JoinKittyWithCode(joinCode);
            }
        }

        internal async Task AddUserClicked()
        {
            string newUser = await _dialogService.GetSingleTextInput("Add a Non-App User", "Enter the name for the person:");

            if (!string.IsNullOrWhiteSpace(newUser))
            {
                await AddNewUser(newUser);
            }
        }

        internal async Task InviteClicked()
        {
            string joinCode = await GetKittyJoinCode();
            await _dialogService.Alert("Join Code", "Advise people to use this code: " + joinCode + " to join your kitty. (expires in 24 hours)", "OK");
        }

        internal Task<string> GetKittyJoinCode()
        {
            return _inviteService.GetJoinCode(_kittyService.Kitty.Id);
        }

        internal async Task JoinKittyWithCode(string joinCode)
        {
            var kittyId = await _inviteService.GetKittyIdWithCode(joinCode);
            if (!string.IsNullOrEmpty(kittyId))
            {
                await _kittyService.AddRegisteredUser(_identityService.LoginData.Email, _identityService.UserDetail.Name);
                await _identityService.AddMeToKitty(kittyId);
            }            
        }

        internal async Task AddNewUser(string newUser)
        {
            await _kittyService.AddNewUser(newUser);
        }
    }
}
