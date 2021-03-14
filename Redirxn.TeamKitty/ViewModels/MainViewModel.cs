using Redirxn.TeamKitty.Models;
using Redirxn.TeamKitty.Services.Application;
using Redirxn.TeamKitty.Services.Logic;
using Redirxn.TeamKitty.Views;
using Splat;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Redirxn.TeamKitty.ViewModels
{
    public class MainViewModel : BaseViewModel
    {        
        private IIdentityService _identityService;
        private IKittyService _kittyService;
        private IInviteService _inviteService;
        private IDialogService _dialogService;
        private IRoutingService _routingService;
        
        private StockItemCount _selectedItem;
        private string _currentKitty = string.Empty;
        public ObservableCollection<StockItemCount> Items { get; }
        public Command LoadItemsCommand { get; }
        public Command<StockItemCount> ItemTapped { get; }

        public string CurrentKitty
        {
            get { return _currentKitty; }
            set { SetProperty(ref _currentKitty, value); }
        }

        public MainViewModel(IIdentityService identityService = null, IKittyService kittyService = null, IDialogService dialogService = null, IInviteService inviteService = null, IRoutingService routingService = null)
        {            
            _identityService = identityService ?? Locator.Current.GetService<IIdentityService>();
            _kittyService = kittyService ?? Locator.Current.GetService<IKittyService>();
            _dialogService = dialogService ?? Locator.Current.GetService<IDialogService>();
            _inviteService = inviteService ?? Locator.Current.GetService<IInviteService>();
            _routingService = routingService ?? Locator.Current.GetService<IRoutingService>();

            Items = new ObservableCollection<StockItemCount>();

            LoadItemsCommand = new Command(async () => await ExecuteLoadItemsCommand());
            ItemTapped = new Command<StockItemCount>(async (item) => await OnItemSelected(item));
        }

        public async Task Init()
        {
            try 
            { 
                IsBusy = true;
                if(_kittyService.Kitty == null && !string.IsNullOrWhiteSpace(_identityService.UserDetail.DefaultKitty))
                {
                    await _kittyService.LoadKitty(_identityService.UserDetail.DefaultKitty);
                }
                CurrentKitty = _kittyService.Kitty?.DisplayName;
                SetSelectedItem(null);

                if (string.IsNullOrEmpty(CurrentKitty))
                {
                    await _routingService.NavigateTo($"{nameof(KittyPage)}");
                }
                else if (string.IsNullOrWhiteSpace(_identityService.UserDetail.Name) || _identityService.UserDetail.Name == _identityService.UserDetail.Id)
                {
                    var newName = await _dialogService.GetSingleTextInput("Name Input", "What name would you like to appear in the kitty as?");
                    if (newName != null)
                    {
                        await _kittyService.RenameMember(_identityService.UserDetail.Id, newName);
                        await _identityService.Rename(newName);
                    }
                }
                else
                {
                    await ExecuteLoadItemsCommand();
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

        async Task ExecuteLoadItemsCommand()
        {
            IsBusy = true;

            try
            {
                Items.Clear();

                if (_kittyService.Kitty != null)
                {
                    foreach (var item in _kittyService.Kitty?.KittyConfig?.StockItems)
                    {
                        var lsl = _kittyService.Kitty.Ledger.Summary.FirstOrDefault(sl => sl.Person.Email == _identityService.LoginData.Email);
                        var purchased = lsl?.Purchases?.FirstOrDefault(p => p.ProductName == item.MainName)?.ProductCount;

                        Items.Add(new StockItemCount
                        {
                            Base = item,
                            MainName = item.MainName,
                            SalePrice = item.SalePrice,
                            Count = purchased ?? 0
                        });
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

        public StockItemCount SelectedItem
        {
            get => _selectedItem;
        }
        public async void SetSelectedItem(StockItemCount value)
        {
            SetProperty(ref _selectedItem, value);
            await OnItemSelected(value);
        }

        async Task OnItemSelected(StockItemCount item)
        {
            if (item == null)
                return;

            const string justMe = "Just Me";
            const string myRound = "It's My Round";
            try
            { 
                var option = await _dialogService.SelectOption($"Tick up a {item.MainName} for:", "Cancel", justMe, myRound);

                switch (option)
                {
                    case justMe:
                        await _kittyService.TickMeASingle(_identityService.LoginData.Email, _identityService.UserDetail.Name, item.Base);
                        IsBusy = true;
                        break;
                    case myRound:
                        await _routingService.NavigateTo($"{nameof(MultiTickPage)}?{nameof(StockItemViewModel.FromMainName)}={item.MainName}");
                        IsBusy = true;
                        break;
                    default:
                        break;
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
    }

    public class StockItemCount
    {
        public StockItem Base { get; set; }
        public string MainName { get; set; }
        public decimal SalePrice { get; set; }
        public int Count { get; set; }
    }
}
