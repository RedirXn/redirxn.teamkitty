using NUnit.Framework;
using Redirxn.TeamKitty.ViewModels;
using Redirxn.TeamKitty.Services.Logic;
using Splat;
using Redirxn.TeamKitty.Models;
using FluentAssertions;
using Redirxn.TeamKitty.Views;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Redirxn.TeamKitty.Tests
{
    [TestFixture]
    public class MainViewModelTests : BaseTest
    {
        private const string myEmail = "me@myplace";
        MainViewModel _vmMain;
        [SetUp]
        public override void Setup()
        {
            base.Setup();

            var fakeLoginData = new NetworkAuthData { Email = myEmail, Id = myEmail };
            Locator.Current.GetService<IIdentityService>().Init("fakeToken", fakeLoginData);
        }

        [Test]
        public async Task KittylessRedirectsToSettingsPage()
        {
            _vmMain = new MainViewModel();

            await _vmMain.Init();

            Routes.WasNavigatedTo(nameof(SettingsPage)).Should().BeTrue();
        }

        [Test]
        public async Task NamelessDemandsNameChange()
        {
            PrepareKitty();            
            const string newName = "New Display Name";
            Dialogs.Make_TextInputReturn(newName);
            _vmMain = new MainViewModel();

            await _vmMain.Init();

            Db.SaveUserDetailToDbUser.Name.Should().Be(newName);            
        }
        [Test]
        public void CanLoadStockItems()
        {
            PrepareKitty();

            _vmMain = new MainViewModel();
            _vmMain.LoadItemsCommand.Execute(null);

            _vmMain.Items.Count.Should().Be(2);
        }
        [Test]
        public void CanTickMeAStockItem()
        {
            PrepareKitty();

            var si = GetFakeAdminKitty().KittyConfig.StockItems.First(si => si.MainName == "Item1");
            var siItem = new StockItemCount { Base = si, MainName = si.MainName, SalePrice = si.SalePrice, Count = 1 };
            Dialogs.Make_SelectOptionReturn("Just Me");
            _vmMain = new MainViewModel();

            _vmMain.ItemTapped.Execute(siItem);

            Db.SaveKittyToDbKitty.Ledger.Transactions[0].TransactionAmount.Should().Be(2.5M);
            Db.SaveKittyToDbKitty.Ledger.Summary.FirstOrDefault(lsl => lsl.Person.Email == myEmail).TotalOwed.Should().Be(2.5M);
        }
        [Test]
        public void MulitpleStockItemsTrackedInLedger()
        {
            PrepareKitty();

            var si1 = GetFakeAdminKitty().KittyConfig.StockItems.First(si => si.MainName == "Item1");
            var siItem1 = new StockItemCount { Base = si1, MainName = si1.MainName, SalePrice = si1.SalePrice, Count = 1 };
            var si2 = GetFakeAdminKitty().KittyConfig.StockItems.First(si => si.MainName == "Item2");
            var siItem2 = new StockItemCount { Base = si2, MainName = si2.MainName, SalePrice = si2.SalePrice, Count = 1 };
            Dialogs.Make_SelectOptionReturn("Just Me");
            _vmMain = new MainViewModel();

            _vmMain.ItemTapped.Execute(siItem1);
            _vmMain.ItemTapped.Execute(siItem1);
            _vmMain.ItemTapped.Execute(siItem1);
            _vmMain.ItemTapped.Execute(siItem1);
            _vmMain.ItemTapped.Execute(siItem2);
            _vmMain.ItemTapped.Execute(siItem2);
            _vmMain.ItemTapped.Execute(siItem2);
            _vmMain.ItemTapped.Execute(siItem2);

            Db.SaveKittyToDbKitty.Ledger.Transactions.Count().Should().Be(8);
            Db.SaveKittyToDbKitty.Ledger.Summary.FirstOrDefault(lsl => lsl.Person.Email == myEmail).TotalOwed.Should().Be(30M);
        }
        [Test]
        public void CanTriggerShoutForOthers()
        {
            PrepareKitty();

            var si = GetFakeAdminKitty().KittyConfig.StockItems.First(si => si.MainName == "Item1");
            var siItem1 = new StockItemCount { Base = si, MainName = si.MainName, SalePrice = si.SalePrice, Count = 1 };
            Dialogs.Make_SelectOptionReturn("It's My Round");
            _vmMain = new MainViewModel();

            _vmMain.ItemTapped.Execute(siItem1);

            Routes.WasNavigatedTo("MultiTickPage?FromMainName=Item1").Should().BeTrue();
        }
        private void PrepareKitty()
        {
            var fakeKitty = GetFakeAdminKitty();
            Db.MakeGetKittyReturn(fakeKitty);
            Locator.Current.GetService<IKittyService>().LoadKitty("anything");
        }
        private Kitty GetFakeAdminKitty()
        {
            return new Kitty
            {
                Administrators = new[] { myEmail },
                KittyConfig = new KittyConfig
                {
                    StockItems = new[]
                    {
                        new StockItem
                        {
                            MainName="Item1",
                            SalePrice=2.5M
                        },
                        new StockItem
                        {
                            MainName="Item2",
                            SalePrice=5M
                        }
                    }
                },
                Ledger = new Ledger
                {
                    Summary = new List<LedgerSummaryLine>
                    {
                        new LedgerSummaryLine
                        {
                            Person = new Member
                            {
                                Email = myEmail
                            }
                        }
                    }
                },
                Id="me|FakeKitty"
            };
        }

    }
}