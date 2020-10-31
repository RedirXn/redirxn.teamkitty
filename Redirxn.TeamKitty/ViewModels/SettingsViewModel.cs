using Redirxn.TeamKitty.Services.Application;
using Redirxn.TeamKitty.Services.Logic;
using Splat;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace Redirxn.TeamKitty.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        IKittyService _kittyService;
        IIdentityService _identityService;
        IInviteService _inviteService;
        IDialogService _dialogService;
        IRoutingService _routingService;

        public ICommand CreateKittyCommand { get; set; }
        public ICommand JoinKittyCommand { get; set; }
        public ICommand InviteCommand { get; set; }
        public ICommand AddUserCommand { get; set; }
        public ICommand ChangeMyNameCommand { get; set; }

        private bool _isAdmin = false;
        public bool IsAdmin
        {
            get => _isAdmin;
            set { SetProperty(ref _isAdmin, value); }
        }
        private bool _kittyExists = true;
        public bool KittyExists
        {
            get => _kittyExists;
            set { SetProperty(ref _kittyExists, value); }
        }

        public SettingsViewModel(IKittyService kittyService = null, IIdentityService identityService = null, IInviteService inviteService = null, IDialogService dialogService = null, IRoutingService routingService = null)
        {
            _kittyService = kittyService ?? Locator.Current.GetService<IKittyService>();
            _identityService = identityService ?? Locator.Current.GetService<IIdentityService>();
            _inviteService = inviteService ?? Locator.Current.GetService<IInviteService>();
            _dialogService = dialogService ?? Locator.Current.GetService<IDialogService>();
            _routingService = routingService ?? Locator.Current.GetService<IRoutingService>();

            CreateKittyCommand = new Command(async () => await CreateNewKitty());
            JoinKittyCommand = new Command(async () => await JoinKitty());
            InviteCommand = new Command(async () => await Invite());
            AddUserCommand = new Command(async () => await AddUser());
            ChangeMyNameCommand = new Command(async () => await ChangeMyName());
        }


        public async Task Init()
        {
            await _kittyService.LoadKitty(_identityService.UserDetail.DefaultKitty);

            IsAdmin = _kittyService.AmIAdmin(_identityService.LoginData.Email);
            KittyExists = !string.IsNullOrEmpty(_kittyService.Kitty.DisplayName);            
        }
        internal async Task CreateNewKitty()
        {
            string newKittyName = await _dialogService.GetSingleTextInput("Create a Kitty", "Enter the name for your new Kitty:");

            if (!string.IsNullOrWhiteSpace(newKittyName))
            {
                await CreateNewKittyWithName(newKittyName);
                await _routingService.NavigateTo($"///main");
            }
        }

        internal async Task CreateNewKittyWithName(string newKittyName)
        {
            if (!_identityService.KittyNameExists(newKittyName))
            {
                var kittyId = await _kittyService.CreateNewKitty(_identityService.LoginData.Email, newKittyName);
                await _identityService.AddMeToKitty(kittyId);
            }
        }
        internal async Task JoinKittyWithCode(string joinCode)
        {
            var kittyId = await _inviteService.GetKittyIdWithCode(joinCode);
            if (!string.IsNullOrEmpty(kittyId))
            {
                await _kittyService.AddRegisteredUser(_identityService.LoginData.Email, _identityService.UserDetail.Name, kittyId);
                await _identityService.AddMeToKitty(kittyId);
            }
        }
        internal async Task JoinKitty()
        {
            string joinCode = await _dialogService.GetSingleTextInput("Join a Kitty", "Enter the code given to you by the Kitty Administrator:");

            if (!string.IsNullOrWhiteSpace(joinCode))
            {
                await JoinKittyWithCode(joinCode);
            }
        }
        internal async Task AddUser()
        {
            string newUser = await _dialogService.GetSingleTextInput("Add a Non-App User", "Enter the name for the person:");
            
            if (!string.IsNullOrWhiteSpace(newUser))
            {
                await AddNewUser(newUser);
            }
        }
        internal async Task ChangeMyName()
        {
            string newName = await _dialogService.GetSingleTextInput("Change My Name", "Enter the new name:");

            if (!string.IsNullOrWhiteSpace(newName))
            {
                await ChangeMyNameTo(newName);
            }
        }

        private async Task ChangeMyNameTo(string newName)
        {
            if(_kittyService.Kitty.Ledger.Summary.FirstOrDefault(lsl => lsl.Person.DisplayName == newName) == null)
            {
                await _kittyService.RenameMember(_identityService.UserDetail.Id, newName);
                await _identityService.Rename(newName);
            }
        }

        internal async Task Invite()
        {
            string joinCode = await GetKittyJoinCode();
            await _dialogService.Alert("Join Code", "Advise people to use this code: " + joinCode + " to join your kitty. (expires in 24 hours)", "OK");
        }
        internal Task<string> GetKittyJoinCode()
        {
            return _inviteService.GetJoinCode(_kittyService.Kitty.Id);
        }
        internal async Task AddNewUser(string newUser)
        {
            await _kittyService.AddNewUser(newUser);
        }
    }
}
