using NUnit.Framework;
using Redirxn.TeamKitty.ViewModels;
using Redirxn.TeamKitty.Services.Logic;
using Splat;
using Redirxn.TeamKitty.Models;
using FluentAssertions;
using Redirxn.TeamKitty.Views;
using System.Collections.Generic;
using System.Linq;

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
        public void KittylessRedirectsToSettingsPage()
        {
            _vmMain = new MainViewModel();

            _vmMain.Init();

            Routes.WasNavigatedTo(nameof(SettingsPage)).Should().BeTrue();
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

            var si = new StockItem
            {
                MainName = "Item1",
                SalePrice = 2.5M
            };
            Dialogs.Make_SelectOptionReturn("Just Me");
            _vmMain = new MainViewModel();

            _vmMain.ItemTapped.Execute(si);

            Db.SaveKittyToDbKitty.Ledger.Transactions[0].TransactionAmount.Should().Be(2.5M);
            Db.SaveKittyToDbKitty.Ledger.Summary.FirstOrDefault(lsl => lsl.Person.Email == myEmail).TotalOwed.Should().Be(2.5M);
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
                }
            };
        }

    }
}