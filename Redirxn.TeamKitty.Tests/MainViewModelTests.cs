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

            Db.MakeGetUserDetailReturn(new UserInfo());
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
        public async Task NoSessionCanInitialiseCorrectly()
        {
            Dialogs.Make_TextInputReturn("DisplayName");
            PrepareKitty();
            _vmMain = new MainViewModel();
            await _vmMain.Init();            
            
            _vmMain.InSession.Should().BeFalse();
            _vmMain.TakingOrders.Should().BeFalse();
            _vmMain.SessionButtonText.Should().Be("Start Session");
            _vmMain.OrderTaker.Should().BeNullOrEmpty();
            _vmMain.MyItem.Should().BeNullOrEmpty();
            _vmMain.IsOrders.Should().BeFalse();
            _vmMain.OrderListText.Should().BeNullOrEmpty();
        }
        [Test]
        public async Task CanStartSession()
        {
            PrepareKitty();
            _vmMain = new MainViewModel();
            await _vmMain.Init();

            _vmMain.SessionToggleCommand.Execute(null);

            _vmMain.InSession.Should().BeTrue();
            _vmMain.TakingOrders.Should().BeFalse();
            _vmMain.SessionButtonText.Should().Be("End Session");
            _vmMain.OrderTaker.Should().BeNullOrEmpty();
            _vmMain.MyItem.Should().BeNullOrEmpty();
            _vmMain.IsOrders.Should().BeFalse();
            _vmMain.OrderListText.Should().BeNullOrEmpty();
        }
        [Test]
        public async Task CanEndSession()
        {
            PrepareKitty();
            _vmMain = new MainViewModel();
            await _vmMain.Init();

            _vmMain.SessionToggleCommand.Execute(null);
            _vmMain.SessionToggleCommand.Execute(null);

            _vmMain.InSession.Should().BeFalse();
            _vmMain.TakingOrders.Should().BeFalse();
            _vmMain.SessionButtonText.Should().Be("Start Session");
            _vmMain.OrderTaker.Should().BeNullOrEmpty();
            _vmMain.MyItem.Should().BeNullOrEmpty();
            _vmMain.IsOrders.Should().BeFalse();
            _vmMain.OrderListText.Should().BeNullOrEmpty();
        }
        [Test]
        public async Task CanStartTakingOrders()
        {
            PrepareKitty();
            _vmMain = new MainViewModel();
            await _vmMain.Init();

            _vmMain.SessionToggleCommand.Execute(null);
            _vmMain.ToTheFridgeCommand.Execute(null);

            _vmMain.InSession.Should().BeTrue();
            _vmMain.TakingOrders.Should().BeTrue();
            _vmMain.SessionButtonText.Should().Be("End Session");
            _vmMain.OrderTaker.Should().Be("DisplayName");
            _vmMain.MyItem.Should().BeNullOrEmpty();
            _vmMain.IsOrders.Should().BeFalse();
            _vmMain.OrderListText.Should().BeNullOrEmpty();
        }
        [Test]
        public async Task CanAddMeToMyOrder()
        {
            PrepareKitty();
            _vmMain = new MainViewModel();
            await _vmMain.Init();
            Dialogs.Make_SelectOptionReturn("Item2");

            _vmMain.SessionToggleCommand.Execute(null);
            _vmMain.ToTheFridgeCommand.Execute(null);
            _vmMain.GrabOneForMeCommand.Execute(null);            

            _vmMain.InSession.Should().BeTrue();
            _vmMain.TakingOrders.Should().BeTrue();
            _vmMain.SessionButtonText.Should().Be("End Session");
            _vmMain.OrderTaker.Should().Be("DisplayName");
            _vmMain.MyItem.Should().Be("Item2");
            _vmMain.IsOrders.Should().BeFalse();
            _vmMain.CanOrderOne.Should().BeFalse();
            _vmMain.OrderListText.Should().Be("");
        }
        [Test]
        public async Task CanCancelMyTripToTheFridge()
        {
            PrepareKitty();
            _vmMain = new MainViewModel();
            await _vmMain.Init();
            Dialogs.Make_SelectOptionReturn("Item2");
            Dialogs.Make_ConfirmReturn(true);

            _vmMain.SessionToggleCommand.Execute(null);
            _vmMain.ToTheFridgeCommand.Execute(null);
            _vmMain.GrabOneForMeCommand.Execute(null);
            _vmMain.CancelFridgeCommand.Execute(null);

            _vmMain.InSession.Should().BeTrue();
            _vmMain.TakingOrders.Should().BeFalse();
            _vmMain.SessionButtonText.Should().Be("End Session");
            _vmMain.OrderTaker.Should().BeNullOrEmpty();
            _vmMain.MyItem.Should().BeNullOrEmpty();
            _vmMain.IsOrders.Should().BeFalse();
            _vmMain.OrderListText.Should().BeNullOrEmpty();
            Db.SaveKittyToDbKitty.Session.Orders.Should().BeEmpty();
        }
        [Test]
        public async Task CanCompleteOrder()
        {
            PrepareKitty();
            _vmMain = new MainViewModel();
            await _vmMain.Init();
            Dialogs.Make_SelectOptionReturn("Item2");

            var expectedText = "Item1 : 0" + System.Environment.NewLine + "Item2 : 1" + System.Environment.NewLine + "DisplayName";

            _vmMain.SessionToggleCommand.Execute(null);
            _vmMain.ToTheFridgeCommand.Execute(null);
            _vmMain.GrabOneForMeCommand.Execute(null);
            _vmMain.CompleteOrderCommand.Execute(null);

            _vmMain.InSession.Should().BeTrue();
            _vmMain.TakingOrders.Should().BeFalse();
            _vmMain.SessionButtonText.Should().Be("End Session");
            _vmMain.OrderTaker.Should().BeNullOrEmpty();
            _vmMain.MyItem.Should().Be("Item2");
            _vmMain.IsOrders.Should().BeTrue();
            _vmMain.OrderListText.Should().Be(expectedText);
            Db.SaveKittyToDbKitty.Session.Orders.Should().NotBeEmpty();
        }

        [Test]
        public async Task CanReceiveOrder()
        {
            PrepareKitty();
            _vmMain = new MainViewModel();
            await _vmMain.Init();
            Dialogs.Make_SelectOptionReturn("Item2");

            _vmMain.SessionToggleCommand.Execute(null);
            _vmMain.ToTheFridgeCommand.Execute(null);
            _vmMain.GrabOneForMeCommand.Execute(null);
            _vmMain.CompleteOrderCommand.Execute(null);
            _vmMain.ItemReceivedCommand.Execute(null);

            _vmMain.InSession.Should().BeTrue();
            _vmMain.TakingOrders.Should().BeFalse();
            _vmMain.SessionButtonText.Should().Be("End Session");
            _vmMain.OrderTaker.Should().BeNullOrEmpty();
            _vmMain.MyItem.Should().BeNullOrEmpty();
            _vmMain.IsOrders.Should().BeFalse();
            _vmMain.OrderListText.Should().BeNullOrEmpty();
            Db.SaveKittyToDbKitty.Session.Orders.Should().BeEmpty();
            Db.SaveKittyToDbKitty.Ledger.Transactions.Count().Should().Be(1);
        }
        [Test]
        public async Task CanAddAnOption()
        {
            PrepareKitty();
            _vmMain = new MainViewModel();
            await _vmMain.Init();
            Dialogs.Make_TextInputReturn("DisplayName");
            Dialogs.Make_TextInputReturn("Option1");

            _vmMain.SessionToggleCommand.Execute(null);
            _vmMain.AddOptionCommand.Execute("Item1");

            Db.SaveKittyToDbKitty.Session.OrderOptions["Item1"].Should().Be("Option1");            
        }
        [Test]
        public async Task CanAddOptions()
        {
            PrepareKitty();
            _vmMain = new MainViewModel();
            await _vmMain.Init();
            Dialogs.Make_TextInputReturn("DisplayName");
            Dialogs.Make_TextInputReturn("Option1");
            Dialogs.Make_TextInputReturn("Option2");
            Dialogs.Make_TextInputReturn("Option3");


            _vmMain.SessionToggleCommand.Execute(null);
            _vmMain.AddOptionCommand.Execute("Item1");
            _vmMain.AddOptionCommand.Execute("Item1");
            _vmMain.AddOptionCommand.Execute("Item2");

            Db.SaveKittyToDbKitty.Session.OrderOptions["Item1"].Should().Be("Option1|Option2");
            Db.SaveKittyToDbKitty.Session.OrderOptions["Item2"].Should().Be("Option3");
        }
        [TestCase("Option1", "Option2","Option3")]
        [TestCase("Option2", "Option1", "Option3")]        
        public async Task CanRemoveAnOption(string remove, string expect1, string expect2)
        {
            PrepareKitty();
            _vmMain = new MainViewModel();
            await _vmMain.Init();
            Dialogs.Make_TextInputReturn("DisplayName");
            Dialogs.Make_TextInputReturn("Option1");
            Dialogs.Make_TextInputReturn("Option2");
            Dialogs.Make_TextInputReturn("Option3");
            Dialogs.Make_TextInputReturn(remove);

            _vmMain.SessionToggleCommand.Execute(null);
            _vmMain.AddOptionCommand.Execute("Item1");
            _vmMain.AddOptionCommand.Execute("Item1");
            _vmMain.AddOptionCommand.Execute("Item2");
            _vmMain.RemoveOptionCommand.Execute("Item1");

            Db.SaveKittyToDbKitty.Session.OrderOptions["Item1"].Should().Be(expect1);
            Db.SaveKittyToDbKitty.Session.OrderOptions["Item2"].Should().Be(expect2);
        }
        [Test]
        public async Task CanEmptyOptions()
        {
            PrepareKitty();
            _vmMain = new MainViewModel();
            await _vmMain.Init();
            Dialogs.Make_TextInputReturn("DisplayName");
            Dialogs.Make_TextInputReturn("Option1");
            Dialogs.Make_TextInputReturn("Option2");
            Dialogs.Make_TextInputReturn("Option3");
            Dialogs.Make_TextInputReturn("Option3");

            _vmMain.SessionToggleCommand.Execute(null);
            _vmMain.AddOptionCommand.Execute("Item1");
            _vmMain.AddOptionCommand.Execute("Item1");
            _vmMain.AddOptionCommand.Execute("Item2");
            _vmMain.RemoveOptionCommand.Execute("Item2");

            Db.SaveKittyToDbKitty.Session.OrderOptions["Item1"].Should().Be("Option1|Option2");
            Db.SaveKittyToDbKitty.Session.OrderOptions.ContainsKey("Item2").Should().BeFalse();
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
                                Email = myEmail,
                                DisplayName = "DisplayName"
                            }
                        }
                    }
                },
                Id="me|FakeKitty"
            };
        }

    }
}