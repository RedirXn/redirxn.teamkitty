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
using System.Windows.Input;
using Xamarin.Forms;

namespace Redirxn.TeamKitty.ViewModels
{
    public class SessionViewModel : BaseViewModel
    {        
        private IIdentityService _identityService;
        private IKittyService _kittyService;
        private IInviteService _inviteService;
        private IDialogService _dialogService;
        private IRoutingService _routingService;
        private ICommunicationService _commsService;
                
        public ICommand SessionToggleCommand { get; }
        public ICommand ToTheFridgeCommand { get; }
        public ICommand CancelFridgeCommand { get; }
        public ICommand CompleteOrderCommand { get; }
        public ICommand GrabOneForMeCommand { get; }
        public ICommand ItemReceivedCommand { get; }
        public ICommand LoadItemsCommand { get; }
        public Command<string> AddOptionCommand { get; }
        public Command<string> RemoveOptionCommand { get; }
        public Command<TickDisplay> ItemTapped { get; }
        public ObservableCollection<TickDisplay> Items { get; }


        private string _currentKitty = string.Empty;
        public string CurrentKitty
        {
            get { return _currentKitty; }
            set { SetProperty(ref _currentKitty, value); }
        }
        private string _sessionButtonText;
        public string SessionButtonText
        {
            get => _sessionButtonText;
            set { SetProperty(ref _sessionButtonText, value); }
        }
        private bool _inSession = false;
        public bool InSession
        {
            get => _inSession;
            set { 
                SetProperty(ref _inSession, value);
                SessionButtonText = value ? "End Session" : "Start Session";
            }
        }
        private bool _takingOrders = false;
        public bool TakingOrders
        {
            get => _takingOrders;
            set { SetProperty(ref _takingOrders, value); }
        }
        private bool _meTakingOrders = false;
        public bool MeTakingOrders
        {
            get => _meTakingOrders;
            set { SetProperty(ref _meTakingOrders, value); }
        }
        private bool _inWaiting = false;
        public bool InWaiting
        {
            get => _inWaiting;
            set { SetProperty(ref _inWaiting, value); }
        }
        private string _myItem;
        public string MyItem
        {
            get => _myItem;
            set { SetProperty(ref _myItem, value); }
        }
        private string _orderTaker;
        public string OrderTaker
        {
            get => _orderTaker;
            set { SetProperty(ref _orderTaker, value); }
        }
        private string _orderListText;
        public string OrderListText
        {
            get => _orderListText;
            set { SetProperty(ref _orderListText, value); }
        }
        private bool _isOrders;
        public bool IsOrders
        {
            get => _isOrders;
            set { SetProperty(ref _isOrders, value); }
        }
        private bool _canStartOrder;
        public bool CanStartOrder
        {
            get => _canStartOrder;
            set { SetProperty(ref _canStartOrder, value); }
        }
        private bool _canOrderOne;
        public bool CanOrderOne
        {
            get => _canOrderOne;
            set { SetProperty(ref _canOrderOne, value); }
        }

        public SessionViewModel(IIdentityService identityService = null, IKittyService kittyService = null, IDialogService dialogService = null, IInviteService inviteService = null, IRoutingService routingService = null, ICommunicationService commsService = null)
        {            
            _identityService = identityService ?? Locator.Current.GetService<IIdentityService>();
            _kittyService = kittyService ?? Locator.Current.GetService<IKittyService>();
            _dialogService = dialogService ?? Locator.Current.GetService<IDialogService>();
            _inviteService = inviteService ?? Locator.Current.GetService<IInviteService>();
            _routingService = routingService ?? Locator.Current.GetService<IRoutingService>();
            _commsService = commsService ?? Locator.Current.GetService<ICommunicationService>();

            Items = new ObservableCollection<TickDisplay>();
            LoadItemsCommand = new Command(async () => await ExecuteLoadItemsCommand());
            ItemTapped = new Command<TickDisplay>(async (i) => await OnItemSelected(i));

            SessionToggleCommand = new Command(async () => await ExecuteSessionToggleCommand());
            ToTheFridgeCommand = new Command(async () => await ExecuteToTheFridgeCommand());
            CancelFridgeCommand = new Command(async () => await ExecuteCancelFridgeCommand());
            CompleteOrderCommand = new Command(async () => await ExecuteCompleteOrderCommand());
            GrabOneForMeCommand = new Command(async () => await ExecuteGrabOneForMeCommand());
            ItemReceivedCommand = new Command(async () => await ExecuteItemReceivedCommand());
            AddOptionCommand = new Command<string>(async (s) => await ExecuteAddOptionCommand(s));
            RemoveOptionCommand = new Command<string>(async (s) => await ExecuteRemoveOptionCommand(s));
        }

        internal string[] GetStockItems()
        {
            return _kittyService.Kitty.KittyConfig.StockItems.Select(si => si.MainName).ToArray();
        }
        internal string[] GetOptionsFor(string stockItem)
        {
            if (_kittyService.Kitty.Session.OrderOptions.ContainsKey(stockItem))
            {
                return _kittyService.Kitty.Session.OrderOptions[stockItem].Split('|');
            }
            return new string[] { };
        }
        private void StateRefresh()
        {
            InSession = _kittyService.Kitty.Session.IsStarted;
            TakingOrders = !string.IsNullOrWhiteSpace(_kittyService.Kitty.Session.PersonTakingOrders);
            OrderTaker = _kittyService.Kitty.Session.PersonTakingOrders;
            var orders = _kittyService.Kitty.Session.Orders;
            var order = orders.SingleOrDefault(o => o.PersonId == _identityService.LoginData.Email);
            InWaiting = order != null;
            MyItem = order?.StockItemName ?? string.Empty;
            IsBusy = TakingOrders;
            MeTakingOrders = OrderTaker == _kittyService.Kitty.Ledger.Summary.Single(lsl => lsl.Person.Email == _identityService.LoginData.Email).Person.DisplayName;
            bool hasOrders = orders.Count > 0;
            IsOrders = hasOrders && string.IsNullOrEmpty(OrderTaker);
            OrderListText = (IsOrders) ? _kittyService.GetOrderListText() : string.Empty;
            CanStartOrder = (!TakingOrders);
            CanOrderOne = TakingOrders && !InWaiting;
        }

        async Task ExecuteLoadItemsCommand()
        {
            try
            {
                Items.Clear();

                foreach (var item in _kittyService.Kitty?.Ledger.Summary)
                {
                    // try to sort by last purchase date desc
                    if (item.Person.Email != _identityService.LoginData.Email) // Remove current USer
                    {
                        ///// Need to match with orders already in session.
                        /*
                         * Indicate who is app user vs non
                         * Pick person, pick drink
                         * indicate who has a drink ordered
                        */
                        var hasOrder = _kittyService.Kitty.Session.Orders.SingleOrDefault(o => o.PersonId == item.Person.Email);
                        Items.Add(new TickDisplay
                        {
                            Id = item.Person.Email,
                            DisplayName = item.Person.DisplayName,
                            Ticked = (hasOrder != null)
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                await _dialogService.Alert("Error", "An Error Occurred", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
        private async Task OnItemSelected(TickDisplay item)
        {
            try
            {
                if (!item.Ticked)
                {
                    string orderItem;
                    if (_kittyService.Kitty.KittyConfig.StockItems.Count() == 1)
                    {
                        orderItem = _kittyService.Kitty.KittyConfig.StockItems.First().MainName;
                    }
                    else
                    {
                        orderItem = await _dialogService.SelectOption("Order: ", "Cancel", _kittyService.Kitty.KittyConfig.StockItems.Select(si => si.MainName).ToArray());
                    }
                    if (orderItem != "Cancel")
                    {
                        string option = string.Empty;
                        if (_kittyService.Kitty.Session.OrderOptions.ContainsKey(orderItem))
                        {
                            option = await _dialogService.SelectOption("Select One", "Cancel", _kittyService.Kitty.Session.OrderOptions[orderItem].Split('|'));
                            if (option == "Cancel")
                            {
                                return;
                            }
                        }
                        await _kittyService.OrderItemInSession(item.Id, item.DisplayName, orderItem, option);
                        item.Ticked = true;
                    }
                }
                else
                {
                    await _kittyService.CancelOrderInSession(item.Id);
                    item.Ticked = false;
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

        private async Task ExecuteSessionToggleCommand()
        {
            try
            {
                if (InSession)
                {
                    await _kittyService.EndSession();
                }
                else
                {
                    await _kittyService.StartSession();
                }

                StateRefresh();
                /*
                 * End Session needs a confirm if open orders
                 * 
                 */
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
        private async Task ExecuteToTheFridgeCommand()
        {
            try
            {
                var p = _kittyService.Kitty.Ledger.Summary.Single(lsl => lsl.Person.Email == _identityService.LoginData.Email).Person;
                await _kittyService.StartTakingOrdersInSession(p.DisplayName);
                StateRefresh();
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


        private async Task ExecuteCancelFridgeCommand()
        {
            try
            {
                var doCancel = await _dialogService.Confirm("Cancel", "Cancel all open orders?", "Yes, Cancel", "No, Keep Taking");
                if (doCancel)
                {
                    await _kittyService.ClearAllOpenOrdersInSession();
                }
                StateRefresh();
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
        private async Task ExecuteCompleteOrderCommand()
        {
            try
            {
                await _kittyService.CloseOrderTakingInSession();


                /*
                 * Show Summary of Order
                 * 
                 * cancel individual line litems on order
                 * 
                 * Show Delivered Button
                 * 
                 */
                StateRefresh();
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
        private async Task ExecuteGrabOneForMeCommand()
        {
            try
            {
                var orderItem = await _dialogService.SelectOption("Order: ", "Cancel", _kittyService.Kitty.KittyConfig.StockItems.Select(si => si.MainName).ToArray());
                if (orderItem == "Cancel")
                {
                    return;
                }

                string option = string.Empty;
                if (_kittyService.Kitty.Session.OrderOptions.ContainsKey(orderItem))
                {
                    option = await _dialogService.SelectOption("Select One", "Cancel", _kittyService.Kitty.Session.OrderOptions[orderItem].Split('|'));
                    if (option == "Cancel")
                    {
                        return;
                    }
                }

                var p = _kittyService.Kitty.Ledger.Summary.First(lsl => lsl.Person.Email == _identityService.LoginData.Email).Person;
                await _kittyService.OrderItemInSession(p.Email, p.DisplayName, orderItem, option);

                StateRefresh();
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
        private async Task ExecuteItemReceivedCommand()
        {
            try
            {
                await _kittyService.ReceivedItemIsSession(_identityService.LoginData.Email);
                StateRefresh();
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
        private async Task ExecuteAddOptionCommand(string sItem)
        {
            try { 
                string newOption = await _dialogService.GetSingleTextInput("Name for " + sItem + " option", "Option:");
                if (!string.IsNullOrWhiteSpace(newOption))
                {
                    if (_kittyService.Kitty.Session.OrderOptions.ContainsKey(sItem))
                    {
                        _kittyService.Kitty.Session.OrderOptions[sItem] += '|' + newOption;
                    }
                    else
                    {
                        _kittyService.Kitty.Session.OrderOptions[sItem] = newOption;
                    }
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
        private async Task ExecuteRemoveOptionCommand(string sItem)
        {
            try
            {
                string newOption = await _dialogService.GetSingleTextInput("Name of " + sItem + " option to remove", "Option:");
                if (!string.IsNullOrWhiteSpace(newOption))
                {
                    if (_kittyService.Kitty.Session.OrderOptions.ContainsKey(sItem))
                    {
                        _kittyService.Kitty.Session.OrderOptions[sItem] = _kittyService.Kitty.Session.OrderOptions[sItem].Replace("|" + newOption, "");
                        _kittyService.Kitty.Session.OrderOptions[sItem] = _kittyService.Kitty.Session.OrderOptions[sItem].Replace(newOption + "|", "");
                        _kittyService.Kitty.Session.OrderOptions[sItem] = _kittyService.Kitty.Session.OrderOptions[sItem].Replace(newOption, "");
                        if (string.IsNullOrWhiteSpace(_kittyService.Kitty.Session.OrderOptions[sItem]))
                        {
                            _kittyService.Kitty.Session.OrderOptions.Remove(sItem);
                        }
                    }

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
}
