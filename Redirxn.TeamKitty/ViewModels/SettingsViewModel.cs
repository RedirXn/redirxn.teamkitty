using Redirxn.TeamKitty.Services.Application;
using Redirxn.TeamKitty.Services.Logic;
using Splat;
using System;
using System.Diagnostics;
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
        public ICommand CombineCommand { get; set; }
        public ICommand ChangeMyNameCommand { get; set; }
        public ICommand ChangeKittyCommand { get; set; }
        public ICommand AssignAdminCommand { get; set; }
        public ICommand ChangeKittyNameCommand { get; set; }
        public ICommand RecalculateKittyCommand { get; set; }

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
        private bool _multipleKitties = true;
        public bool MultipleKitties
        {
            get => _multipleKitties;
            set { SetProperty(ref _multipleKitties, value); }
        }
        private string _currentKitty = string.Empty;
        public string CurrentKitty
        {
            get { return _currentKitty; }
            set { SetProperty(ref _currentKitty, value); }
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
            CombineCommand = new Command(async () => await Combine());
            AddUserCommand = new Command(async () => await AddUser());
            ChangeMyNameCommand = new Command(async () => await ChangeMyName());
            ChangeKittyCommand = new Command(async () => await ChangeKitty());
            AssignAdminCommand = new Command(async () => await AssignAdmin()); 
            ChangeKittyNameCommand = new Command(async () => await ChangeKittyName());
            RecalculateKittyCommand = new Command(async () => await RecalculateKitty());

        }

        public async Task Init()
        {
            try
            { 
                await _kittyService.LoadKitty(_identityService.UserDetail.DefaultKitty);

                IsAdmin = _kittyService.AmIAdmin(_identityService.LoginData.Email);
                KittyExists = !string.IsNullOrEmpty(_kittyService.Kitty.DisplayName);
                MultipleKitties = _identityService.UserDetail.KittyNames.Count > 1;
                CurrentKitty = _kittyService.Kitty?.DisplayName;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                await _dialogService.Alert("Error", "An Error Occurred", "OK");
            }
        }
        internal async Task CreateNewKitty()
        {
            try
            { 
                string newKittyName = await _dialogService.GetSingleTextInput("Create a Kitty", "Enter the name for your new Kitty:");

                if (!string.IsNullOrWhiteSpace(newKittyName))
                {
                    await CreateNewKittyWithName(newKittyName);
                    await _routingService.NavigateTo($"///main");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                await _dialogService.Alert("Error", "An Error Occurred", "OK");
            }
        }

        internal async Task CreateNewKittyWithName(string newKittyName)
        {
            if (!_identityService.KittyNameExists(newKittyName))
            {
                var kittyId = await _kittyService.CreateNewKitty(_identityService.LoginData.Email, _identityService.LoginData.Name, newKittyName);
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
            try 
            { 
                string joinCode = await _dialogService.GetSingleTextInput("Join a Kitty", "Enter the code given to you by the Kitty Administrator:");

                if (!string.IsNullOrWhiteSpace(joinCode))
                {
                    await JoinKittyWithCode(joinCode);
                    await _routingService.NavigateTo($"///main");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                await _dialogService.Alert("Error", "An Error Occurred", "OK");
            }
        }
        internal async Task AddUser()
        {
            try
            { 
                string newUser = await _dialogService.GetSingleTextInput("Add a Non-App User", "Enter the name for the person:");
            
                if (!string.IsNullOrWhiteSpace(newUser))
                {
                    await AddNewUser(newUser);
                }
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

                if (!string.IsNullOrWhiteSpace(newName))
                {
                    await ChangeMyNameTo(newName);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                await _dialogService.Alert("Error", "An Error Occurred", "OK");
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
            try
            { 
                string joinCode = await GetKittyJoinCode();
                await _dialogService.Alert("Join Code", joinCode + Environment.NewLine + "Advise people to use this code to join your kitty. (expires in 24 hours)", "OK");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                await _dialogService.Alert("Error", "An Error Occurred", "OK");
            }
        }
        internal async Task Combine()
        {
            try
            {
                var userlist = _kittyService.Kitty.Ledger.Summary.Select(s => s.Person);
                if (userlist.Count() < 2) return;
                var keep = await _dialogService.SelectOption("Select the user that will stay", "Cancel", userlist.Select(u => u.DisplayName).ToArray());
                if (keep != "Cancel")
                {
                    var keepUser = userlist.First(ul => ul.DisplayName == keep);
                    var absorb = await _dialogService.SelectOption("Select the user that will be removed", "Cancel", userlist.Where(u => u != keepUser).Select(u => u.DisplayName).ToArray());
                    if (!string.IsNullOrEmpty(absorb))
                    {
                        var absorbUser = userlist.First(ul => ul.DisplayName == absorb);
                        await _kittyService.CombineUsers(keepUser.Email, absorbUser.Email);
                    }
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                await _dialogService.Alert("Error", "An Error Occurred", "OK");
            }
        }
        internal Task<string> GetKittyJoinCode()
        {
            return _inviteService.GetJoinCode(_kittyService.Kitty.Id);
        }
        internal async Task AddNewUser(string newUser)
        {
            await _kittyService.AddNewUser(newUser);
        }
        private async Task ChangeKitty()
        {
            try
            {
                var kitties = _identityService.UserDetail.KittyNames;
                var displayKitties = kitties.Select(k => k.Split('|')[1]).ToArray();
                string nextKittyDisplay = await _dialogService.SelectOption("Choose Kitty", "Cancel", displayKitties);

                if (!string.IsNullOrWhiteSpace(nextKittyDisplay))
                {
                    var nextKitty = kitties.First(k => k.EndsWith("|" + nextKittyDisplay));
                    await _kittyService.LoadKitty(nextKitty);
                    await _identityService.SetDefaultKitty(nextKitty);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                await _dialogService.Alert("Error", "An Error Occurred", "OK");
            }
        }
        private async Task AssignAdmin()
        {
            try
            {
                //TODO
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                await _dialogService.Alert("Error", "An Error Occurred", "OK");
            }
        }
        private async Task ChangeKittyName()
        {
            try
            {
                //TODO
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                await _dialogService.Alert("Error", "An Error Occurred", "OK");
            }
        }
        private async Task RecalculateKitty()
        {
            try
            {
                await _kittyService.RecalculateKitty();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                await _dialogService.Alert("Error", "An Error Occurred", "OK");
            }
        }

    }
}
