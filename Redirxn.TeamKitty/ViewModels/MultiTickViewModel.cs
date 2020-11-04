﻿using Redirxn.TeamKitty.Services.Application;
using Redirxn.TeamKitty.Services.Logic;
using Splat;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace Redirxn.TeamKitty.ViewModels
{
    [QueryProperty(nameof(FromMainName), nameof(FromMainName))]

    public class MultiTickViewModel : BaseViewModel
    {
        IKittyService _kittyService;
        IRoutingService _routingService;

        string _itemName;
        public string FromMainName
        {
            get { return _itemName; }
            set { _itemName = value; }
        }
        string _confirmText = "None";
        public string ConfirmText
        {
            get { return _confirmText; }
            set { SetProperty(ref _confirmText, value); }
        }

        public ICommand ConfirmCommand { get; }
        public ICommand LoadItemsCommand { get; }
        public Command<TickDisplay> ItemTapped { get; }
        public ObservableCollection<TickDisplay> Items { get; }

        public MultiTickViewModel(IKittyService kittyService = null, IRoutingService routingService = null)
        {
            _kittyService = kittyService ?? Locator.Current.GetService<IKittyService>();
            _routingService = routingService ?? Locator.Current.GetService<IRoutingService>();

            Items = new ObservableCollection<TickDisplay>();

            LoadItemsCommand = new Command(() => ExecuteLoadItemsCommand());
            ItemTapped = new Command<TickDisplay>(OnItemSelected);
            ConfirmCommand = new Command(async () => await Confirmed());
        }
        public void OnAppearing()
        {
            // This "IsBusy" assignment is what triggers the refresh which in turn calls to load the items.
            IsBusy = true;
        }

        private async Task Confirmed()
        {
            var people = new List<string>();

            foreach (var i in Items.Where(t => t.Ticked))
            {
                people.Add(i.DisplayName);
            };
            if (people.Count() > 0)
            {
                await _kittyService.TickMultiplePeople(people, _kittyService.Kitty.KittyConfig.StockItems.FirstOrDefault(s => s.MainName == _itemName));
                await _routingService.GoBack();
            }
        }

        private void OnItemSelected(TickDisplay item)
        {
            item.Ticked = !item.Ticked;

            var count = Items.Where(t => t.Ticked).Count();
            var si = _kittyService.Kitty.KittyConfig.StockItems.FirstOrDefault(s => s.MainName == _itemName);

            if (count == 0)
            {
                ConfirmText = "None";
            }
            else if (count == 1)
            {
                ConfirmText = $"1 {si.MainName}";
            }
            else
            {
                ConfirmText = $"{count} {si.PluralName}";
            }
        }

        void ExecuteLoadItemsCommand()
        {
            IsBusy = true;

            try
            {
                Items.Clear();

                foreach (var item in _kittyService.Kitty?.Ledger.Summary)
                {
                    Items.Add(new TickDisplay
                    {
                        DisplayName = item.Person.DisplayName,
                        Ticked = false
                    });
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
    }
}