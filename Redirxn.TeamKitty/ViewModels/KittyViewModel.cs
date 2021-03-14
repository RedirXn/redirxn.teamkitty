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
using System.Linq;
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
        IInviteService _inviteService;

        public ICommand LoadItemsCommand { get; }
        public ICommand AdjustCommand { get; set; }
        public ICommand ChangeKittyCommand { get; set; }
        public ICommand CreateKittyCommand { get; set; }
        public ICommand JoinKittyCommand { get; set; }
        public ICommand InviteCommand { get; set; }
        public ICommand AddUserCommand { get; set; }
        public ICommand CombineCommand { get; set; }
        public ICommand AssignAdminCommand { get; set; }
        public ICommand ChangeKittyNameCommand { get; set; }
        public ICommand RecalculateKittyCommand { get; set; }
        public ICommand CarryoverKittyCommand { get; set; }

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
        public KittyViewModel(IRoutingService navigationService = null, IKittyService kittyService = null, IIdentityService identityService = null, IDialogService dialogService = null, IInviteService inviteService = null)
        {
            _routingService = navigationService ?? Locator.Current.GetService<IRoutingService>();
            _kittyService = kittyService ?? Locator.Current.GetService<IKittyService>();
            _identityService = identityService ?? Locator.Current.GetService<IIdentityService>();
            _dialogService = dialogService ?? Locator.Current.GetService<IDialogService>();
            _inviteService = inviteService ?? Locator.Current.GetService<IInviteService>();

            Items = new ObservableCollection<LedgerSummaryLine>();

            LoadItemsCommand = new Command(async () => await ExecuteLoadItemsCommand());
            ItemTapped = new Command<LedgerSummaryLine>(OnItemSelected);
            AdjustCommand = new Command(async () => await AdjustmentRequest());
            ChangeKittyCommand = new Command(async () => await ChangeKitty());
            CreateKittyCommand = new Command(async () => await CreateNewKitty());
            JoinKittyCommand = new Command(async () => await JoinKitty()); 
            InviteCommand = new Command(async () => await Invite());
            CombineCommand = new Command(async () => await Combine());
            AddUserCommand = new Command(async () => await AddUser());
            AssignAdminCommand = new Command(async () => await AssignAdmin());
            ChangeKittyNameCommand = new Command(async () => await ChangeKittyName());
            RecalculateKittyCommand = new Command(async () => await RecalculateKitty());
            CarryoverKittyCommand = new Command(async () => await CarryoverKitty());

            IsAdmin = _kittyService.AmIAdmin(_identityService.LoginData.Email);
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
                    OnAppearing();
                }
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

        internal async Task<string> CreateNewKittyWithName(string newKittyName)
        {
            if (!_identityService.KittyNameExists(newKittyName))
            {
                var kittyId = await _kittyService.CreateNewKitty(_identityService.LoginData.Email, _identityService.LoginData.Name, newKittyName);
                await _identityService.AddMeToKitty(kittyId);
                return kittyId;
            }
            return null;
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
            OnAppearing(); 
        }
        private async Task AssignAdmin()
        {
            try
            {
                var users = _kittyService.GetNonAdminAppUsers();
                var user = await _dialogService.SelectOption("Select Person to Make Admin", "Cancel", users.Select(u => u.Item2).ToArray());
                if (user != "Cancel")
                {
                    var adminUser = users.First(u => u.Item2 == user);
                    await _kittyService.MakeUserAdmin(adminUser.Item1);
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
        private async Task ChangeKittyName()
        {
            try
            {
                //TODO
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
        private async Task RecalculateKitty()
        {
            try
            {
                await _kittyService.RecalculateKitty();
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

        private async Task CarryoverKitty()
        {
            try
            {
                var carryType = await _dialogService.SelectOption("Create a New Kitty or Combine with Existing", "Cancel", new[] { "New Kitty", "Combine" });
                if (carryType == "Cancel")
                {
                    return;
                }
                var oldKittyId = _kittyService.Kitty.Id;
                if (carryType == "New Kitty")
                {                    
                    var newKittyName = await _dialogService.GetSingleTextInput("New Kitty Name", "Enter a name for the new Kitty:");
                    if (string.IsNullOrWhiteSpace(newKittyName))
                    {
                        return;
                    }
                    var newKittyId = await CreateNewKittyWithName(newKittyName);
                    await CombineKitties(oldKittyId, newKittyId);
                }
                if (carryType == "Combine")
                {
                    var kitties = _identityService.UserDetail.KittyNames;
                    var displayKitties = kitties.Select(k => k.Split('|')[1]).ToArray();
                    var newKittyName = await _dialogService.SelectOption("Select the Kitty to Carry Over to:", "Cancel", displayKitties);
                    if (newKittyName == "Cancel")
                    {
                        return;
                    }
                    var newKitty = kitties.First(k => k.EndsWith("|" + newKittyName));
                    await CombineKitties(oldKittyId, newKitty);
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

        private async Task CombineKitties(string oldKittyId, string newKittyId)
        {
            await _kittyService.CombineKitties(oldKittyId, newKittyId);
            await _identityService.SetDefaultKitty(newKittyId);
            OnAppearing();
        }
    }
}
