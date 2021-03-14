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
    public class SessionViewModelTests : BaseTest
    {
        private const string myEmail = "me@myplace";
        SessionViewModel _vmSession;
        [SetUp]
        public override void Setup()
        {
            base.Setup();

            Db.MakeGetUserDetailReturn(new UserInfo());
            var fakeLoginData = new NetworkAuthData { Email = myEmail, Id = myEmail };
            Locator.Current.GetService<IIdentityService>().Init("fakeToken", fakeLoginData);
        }
        [Test]
        public async Task NoSessionCanInitialiseCorrectly()
        {
            await PrepareKitty();
            
            _vmSession.InSession.Should().BeFalse();
            _vmSession.TakingOrders.Should().BeFalse();
            _vmSession.SessionButtonText.Should().Be("Start Session");
            _vmSession.OrderTaker.Should().BeNullOrEmpty();
            _vmSession.MyItem.Should().BeNullOrEmpty();
            _vmSession.IsOrders.Should().BeFalse();
            _vmSession.OrderListText.Should().BeNullOrEmpty();
        }
        [Test]
        public async Task CanStartSession()
        {
            await PrepareKitty();

            _vmSession.SessionToggleCommand.Execute(null);

            _vmSession.InSession.Should().BeTrue();
            _vmSession.TakingOrders.Should().BeFalse();
            _vmSession.SessionButtonText.Should().Be("End Session");
            _vmSession.OrderTaker.Should().BeNullOrEmpty();
            _vmSession.MyItem.Should().BeNullOrEmpty();
            _vmSession.IsOrders.Should().BeFalse();
            _vmSession.OrderListText.Should().BeNullOrEmpty();
        }
        [Test]
        public async Task CanEndSession()
        {
            await PrepareKitty();

            _vmSession.SessionToggleCommand.Execute(null);
            _vmSession.SessionToggleCommand.Execute(null);

            _vmSession.InSession.Should().BeFalse();
            _vmSession.TakingOrders.Should().BeFalse();
            _vmSession.SessionButtonText.Should().Be("Start Session");
            _vmSession.OrderTaker.Should().BeNullOrEmpty();
            _vmSession.MyItem.Should().BeNullOrEmpty();
            _vmSession.IsOrders.Should().BeFalse();
            _vmSession.OrderListText.Should().BeNullOrEmpty();
        }
        [Test]
        public async Task CanStartTakingOrders()
        {
            await PrepareKitty();

            _vmSession.SessionToggleCommand.Execute(null);
            _vmSession.ToTheFridgeCommand.Execute(null);

            _vmSession.InSession.Should().BeTrue();
            _vmSession.TakingOrders.Should().BeTrue();
            _vmSession.SessionButtonText.Should().Be("End Session");
            _vmSession.OrderTaker.Should().Be("DisplayName");
            _vmSession.MyItem.Should().BeNullOrEmpty();
            _vmSession.IsOrders.Should().BeFalse();
            _vmSession.OrderListText.Should().BeNullOrEmpty();
        }
        [Test]
        public async Task CanAddMeToMyOrder()
        {
            await PrepareKitty();
            Dialogs.Make_SelectOptionReturn("Item2");

            _vmSession.SessionToggleCommand.Execute(null);
            _vmSession.ToTheFridgeCommand.Execute(null);
            _vmSession.GrabOneForMeCommand.Execute(null);            

            _vmSession.InSession.Should().BeTrue();
            _vmSession.TakingOrders.Should().BeTrue();
            _vmSession.SessionButtonText.Should().Be("End Session");
            _vmSession.OrderTaker.Should().Be("DisplayName");
            _vmSession.MyItem.Should().Be("Item2");
            _vmSession.IsOrders.Should().BeFalse();
            _vmSession.CanOrderOne.Should().BeFalse();
            _vmSession.OrderListText.Should().Be("");
        }
        [Test]
        public async Task CanCancelMyTripToTheFridge()
        {
            await PrepareKitty();
            Dialogs.Make_SelectOptionReturn("Item2");
            Dialogs.Make_ConfirmReturn(true);

            _vmSession.SessionToggleCommand.Execute(null);
            _vmSession.ToTheFridgeCommand.Execute(null);
            _vmSession.GrabOneForMeCommand.Execute(null);
            _vmSession.CancelFridgeCommand.Execute(null);

            _vmSession.InSession.Should().BeTrue();
            _vmSession.TakingOrders.Should().BeFalse();
            _vmSession.SessionButtonText.Should().Be("End Session");
            _vmSession.OrderTaker.Should().BeNullOrEmpty();
            _vmSession.MyItem.Should().BeNullOrEmpty();
            _vmSession.IsOrders.Should().BeFalse();
            _vmSession.OrderListText.Should().BeNullOrEmpty();
            Db.SaveKittyToDbKitty.Session.Orders.Should().BeEmpty();
        }
        [Test]
        public async Task CanCompleteOrder()
        {
            await PrepareKitty();
            Dialogs.Make_SelectOptionReturn("Item2");

            var expectedText = "Item1 : 0" + System.Environment.NewLine + "Item2 : 1" + System.Environment.NewLine + "DisplayName";

            _vmSession.SessionToggleCommand.Execute(null);
            _vmSession.ToTheFridgeCommand.Execute(null);
            _vmSession.GrabOneForMeCommand.Execute(null);
            _vmSession.CompleteOrderCommand.Execute(null);

            _vmSession.InSession.Should().BeTrue();
            _vmSession.TakingOrders.Should().BeFalse();
            _vmSession.SessionButtonText.Should().Be("End Session");
            _vmSession.OrderTaker.Should().BeNullOrEmpty();
            _vmSession.MyItem.Should().Be("Item2");
            _vmSession.IsOrders.Should().BeTrue();
            _vmSession.OrderListText.Should().Be(expectedText);
            Db.SaveKittyToDbKitty.Session.Orders.Should().NotBeEmpty();
        }

        [Test]
        public async Task CanReceiveOrder()
        {
            await PrepareKitty();
            Dialogs.Make_SelectOptionReturn("Item2");

            _vmSession.SessionToggleCommand.Execute(null);
            _vmSession.ToTheFridgeCommand.Execute(null);
            _vmSession.GrabOneForMeCommand.Execute(null);
            _vmSession.CompleteOrderCommand.Execute(null);
            _vmSession.ItemReceivedCommand.Execute(null);

            _vmSession.InSession.Should().BeTrue();
            _vmSession.TakingOrders.Should().BeFalse();
            _vmSession.SessionButtonText.Should().Be("End Session");
            _vmSession.OrderTaker.Should().BeNullOrEmpty();
            _vmSession.MyItem.Should().BeNullOrEmpty();
            _vmSession.IsOrders.Should().BeFalse();
            _vmSession.OrderListText.Should().BeNullOrEmpty();
            Db.SaveKittyToDbKitty.Session.Orders.Should().BeEmpty();
            Db.SaveKittyToDbKitty.Ledger.Transactions.Count().Should().Be(1);
        }
        [Test]
        public async Task CanAddAnOption()
        {
            await PrepareKitty();
            Dialogs.Make_TextInputReturn("Option1");

            _vmSession.SessionToggleCommand.Execute(null);
            _vmSession.AddOptionCommand.Execute("Item1");

            Db.SaveKittyToDbKitty.Session.OrderOptions["Item1"].Should().Be("Option1");            
        }
        [Test]
        public async Task CanAddOptions()
        {
            await PrepareKitty();
            Dialogs.Make_TextInputReturn("Option1");
            Dialogs.Make_TextInputReturn("Option2");
            Dialogs.Make_TextInputReturn("Option3");


            _vmSession.SessionToggleCommand.Execute(null);
            _vmSession.AddOptionCommand.Execute("Item1");
            _vmSession.AddOptionCommand.Execute("Item1");
            _vmSession.AddOptionCommand.Execute("Item2");

            Db.SaveKittyToDbKitty.Session.OrderOptions["Item1"].Should().Be("Option1|Option2");
            Db.SaveKittyToDbKitty.Session.OrderOptions["Item2"].Should().Be("Option3");
        }
        [TestCase("Option1", "Option2","Option3")]
        [TestCase("Option2", "Option1", "Option3")]        
        public async Task CanRemoveAnOption(string remove, string expect1, string expect2)
        {
            await PrepareKitty();
            Dialogs.Make_TextInputReturn("Option1");
            Dialogs.Make_TextInputReturn("Option2");
            Dialogs.Make_TextInputReturn("Option3");
            Dialogs.Make_TextInputReturn(remove);

            _vmSession.SessionToggleCommand.Execute(null);
            _vmSession.AddOptionCommand.Execute("Item1");
            _vmSession.AddOptionCommand.Execute("Item1");
            _vmSession.AddOptionCommand.Execute("Item2");
            _vmSession.RemoveOptionCommand.Execute("Item1");

            Db.SaveKittyToDbKitty.Session.OrderOptions["Item1"].Should().Be(expect1);
            Db.SaveKittyToDbKitty.Session.OrderOptions["Item2"].Should().Be(expect2);
        }
        [Test]
        public async Task CanEmptyOptions()
        {
            await PrepareKitty();
            Dialogs.Make_TextInputReturn("Option1");
            Dialogs.Make_TextInputReturn("Option2");
            Dialogs.Make_TextInputReturn("Option3");
            Dialogs.Make_TextInputReturn("Option3");

            _vmSession.SessionToggleCommand.Execute(null);
            _vmSession.AddOptionCommand.Execute("Item1");
            _vmSession.AddOptionCommand.Execute("Item1");
            _vmSession.AddOptionCommand.Execute("Item2");
            _vmSession.RemoveOptionCommand.Execute("Item2");

            Db.SaveKittyToDbKitty.Session.OrderOptions["Item1"].Should().Be("Option1|Option2");
            Db.SaveKittyToDbKitty.Session.OrderOptions.ContainsKey("Item2").Should().BeFalse();
        }
        private async Task PrepareKitty()
        {
            var fakeKitty = GetFakeAdminKitty();
            Db.MakeGetKittyReturn(fakeKitty);
            await Locator.Current.GetService<IKittyService>().LoadKitty("anything");
            _vmSession = new SessionViewModel();            
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