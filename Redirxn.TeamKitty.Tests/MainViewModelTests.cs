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
        public void KittylessRedirectsToSettingsPage()
        {
            _vmMain = new MainViewModel();

            _vmMain.Init();

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
            Dialogs.Make_SelectOptionReturn("Just Me");
            _vmMain = new MainViewModel();

            _vmMain.ItemTapped.Execute(si);

            Db.SaveKittyToDbKitty.Ledger.Transactions[0].TransactionAmount.Should().Be(2.5M);
            Db.SaveKittyToDbKitty.Ledger.Summary.FirstOrDefault(lsl => lsl.Person.Email == myEmail).TotalOwed.Should().Be(2.5M);
        }
        [Test]
        public void MulitpleStockItemsTrackedInLedger()
        {
            PrepareKitty();

            var si1 = GetFakeAdminKitty().KittyConfig.StockItems.First(si => si.MainName == "Item1");
            var si2 = GetFakeAdminKitty().KittyConfig.StockItems.First(si => si.MainName == "Item2");
            Dialogs.Make_SelectOptionReturn("Just Me");
            _vmMain = new MainViewModel();

            _vmMain.ItemTapped.Execute(si1);
            _vmMain.ItemTapped.Execute(si1);
            _vmMain.ItemTapped.Execute(si1);
            _vmMain.ItemTapped.Execute(si1);
            _vmMain.ItemTapped.Execute(si2);
            _vmMain.ItemTapped.Execute(si2);
            _vmMain.ItemTapped.Execute(si2);
            _vmMain.ItemTapped.Execute(si2);

            Db.SaveKittyToDbKitty.Ledger.Transactions.Count().Should().Be(8);
            Db.SaveKittyToDbKitty.Ledger.Summary.FirstOrDefault(lsl => lsl.Person.Email == myEmail).TotalOwed.Should().Be(30M);
        }
        [Test]
        public void CanTriggerShoutForOthers()
        {
            PrepareKitty();

            var si = GetFakeAdminKitty().KittyConfig.StockItems.First(si => si.MainName == "Item1");
            Dialogs.Make_SelectOptionReturn("It's My Round");
            _vmMain = new MainViewModel();

            _vmMain.ItemTapped.Execute(si);

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
    [TestFixture]
    public class MultiTickViewModelTests : BaseTest
    {
        private const string myEmail = "me@myplace";
        MultiTickViewModel _vmMultiTick;

        [SetUp]
        public override void Setup()
        {
            base.Setup();

            var fakeLoginData = new NetworkAuthData { Email = myEmail, Id = myEmail };
            Locator.Current.GetService<IIdentityService>().Init("fakeToken", fakeLoginData);
        }
        [Test]
        public void CanSeeMembers()
        {
            PrepareKitty();
            _vmMultiTick = new MultiTickViewModel();
            _vmMultiTick.FromMainName = "Item1";

            _vmMultiTick.LoadItemsCommand.Execute(null);

            _vmMultiTick.Items.Count().Should().Be(3);
        }
        [Test]
        public void CanSelectSomeone()
        {
            PrepareKitty();
            _vmMultiTick = new MultiTickViewModel();
            _vmMultiTick.FromMainName = "Item1";
            
            _vmMultiTick.LoadItemsCommand.Execute(null);
            _vmMultiTick.ItemTapped.Execute(_vmMultiTick.Items[1]);

            _vmMultiTick.Items.Where(t => t.Ticked).Count().Should().Be(1);
            _vmMultiTick.ConfirmText.Should().Be("1 Item1");
        }
        [Test]
        public void CanUnSelectSomeone()
        {
            PrepareKitty();
            _vmMultiTick = new MultiTickViewModel();
            _vmMultiTick.FromMainName = "Item1";

            _vmMultiTick.LoadItemsCommand.Execute(null);
            _vmMultiTick.ItemTapped.Execute(_vmMultiTick.Items[1]);
            _vmMultiTick.ItemTapped.Execute(_vmMultiTick.Items[1]);

            _vmMultiTick.Items.Where(t => t.Ticked).Count().Should().Be(0);
            _vmMultiTick.ConfirmText.Should().Be("None");
        }
        [Test]
        public void CanSelectMultiple()
        {
            PrepareKitty();
            _vmMultiTick = new MultiTickViewModel();
            _vmMultiTick.FromMainName = "Item1";

            _vmMultiTick.LoadItemsCommand.Execute(null);
            _vmMultiTick.ItemTapped.Execute(_vmMultiTick.Items[1]);
            _vmMultiTick.ItemTapped.Execute(_vmMultiTick.Items[2]);

            _vmMultiTick.Items.Where(t => t.Ticked).Count().Should().Be(2);
            _vmMultiTick.ConfirmText.Should().Be("2 Items");
        }
        [Test]
        public void CanShout()
        {
            PrepareKitty();
            _vmMultiTick = new MultiTickViewModel();
            _vmMultiTick.FromMainName = "Item1";

            _vmMultiTick.LoadItemsCommand.Execute(null);
            _vmMultiTick.ItemTapped.Execute(_vmMultiTick.Items[1]);
            _vmMultiTick.ItemTapped.Execute(_vmMultiTick.Items[2]);

            _vmMultiTick.ConfirmCommand.Execute(null);

            Db.SaveKittyToDbKitty.Ledger.Transactions.Count().Should().Be(2);
            Routes.WasGoBackCalled().Should().BeTrue();
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
                            SalePrice=2.5M,
                            PluralName="Items"
                        },
                        new StockItem
                        {
                            MainName="Item2",
                            SalePrice=5M,
                            PluralName="Twosies"
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
                                Email = myEmail,
                                DisplayName = "Alfred"
                            }
                        },
                        new LedgerSummaryLine
                        {
                            Person = new Member
                            {
                                Email = "them@theirPlace",
                                DisplayName = "Bruce"
                            }
                        },
                        new LedgerSummaryLine
                        {
                            Person = new Member
                            {
                                Email = "b@tCave",
                                DisplayName = "Dick"
                            }
                        }
                    }
                }
            };
        }

    }
}