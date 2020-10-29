using Redirxn.TeamKitty.Models;
using Redirxn.TeamKitty.Services.Application;
using Redirxn.TeamKitty.Services.Logic;
using Splat;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Redirxn.TeamKitty.ViewModels
{
    class MainViewModel : BaseViewModel
    {        
        private IIdentityService _identityService;
        private IKittyService _kittyService;
        private IDialogService _dialogService;
        
        private StockItem _selectedItem;
        private string _currentKitty = string.Empty;
        public ObservableCollection<StockItem> Items { get; }
        public Command LoadItemsCommand { get; }
        public Command<StockItem> ItemTapped { get; }

        public string CurrentKitty
        {
            get { return _currentKitty; }
            set { SetProperty(ref _currentKitty, value); }
        }

        public MainViewModel(IIdentityService identityService = null, IKittyService kittyService = null, IDialogService dialogService = null)
        {            
            _identityService = identityService ?? Locator.Current.GetService<IIdentityService>();
            _kittyService = kittyService ?? Locator.Current.GetService<IKittyService>();
            _dialogService = dialogService ?? Locator.Current.GetService<IDialogService>();

            Items = new ObservableCollection<StockItem>();

            LoadItemsCommand = new Command(() => ExecuteLoadItemsCommand());
            ItemTapped = new Command<StockItem>(OnItemSelected);
        }

        internal async Task Init()
        {
            IsBusy = true;
            await _kittyService.LoadKitty(_identityService.UserDetail.DefaultKitty);
            CurrentKitty = _kittyService.Kitty?.DisplayName;
            SelectedItem = null;

            await EnsureUserBelongsToAKitty();

            ExecuteLoadItemsCommand();
        }
        private async Task EnsureUserBelongsToAKitty()
        {
            if (string.IsNullOrEmpty(CurrentKitty))
            {
                string createKitty = "Create a New Kitty";
                string action = await _dialogService.SelectOption("You do not belong to a kitty", "Cancel", createKitty, "Join an Existing Kitty");                
                
                if (action == createKitty)
                {                    
                    string newKittyName = await _dialogService.GetSingleTextInput("Create New Kitty", "Enter a name for the new kitty:");
                    if (newKittyName != null)
                    {
                        await CreateNewKitty(newKittyName);
                    }
                }
            }
        }

        void ExecuteLoadItemsCommand()
        {
            IsBusy = true;

            try
            {
                Items.Clear();

                foreach (var item in _kittyService.Kitty.KittyConfig.StockItems)
                {
                    Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        internal async Task CreateNewKitty(string newKittyName)
        {
            if (!_identityService.KittyNameExists(newKittyName))
            {                
                var kittyId = await _kittyService.CreateNewKitty(_identityService.LoginData.Email, newKittyName);
                await _identityService.AddMeToKitty(kittyId);
                CurrentKitty = _kittyService.Kitty.DisplayName;                
            }
        }

        public StockItem SelectedItem
        {
            get => _selectedItem;
            set
            {
                SetProperty(ref _selectedItem, value);
                OnItemSelected(value);
            }
        }

        async void OnItemSelected(StockItem item)
        {
            if (item == null)
                return;

            const string justMe = "Just Me";
            const string myRound = "It's My Round";
            var option = await _dialogService.SelectOption($"Tick up a {item.MainName} for:", "Cancel", justMe, myRound);

            switch (option)
            {
                case justMe:
                    await _kittyService.TickMeASingle(_identityService.LoginData.Email, _identityService.UserDetail.Name, item);
                    break;
                case myRound:
                    // people select page
                    break;
                default:
                    break;
            }
        }
    }
}
