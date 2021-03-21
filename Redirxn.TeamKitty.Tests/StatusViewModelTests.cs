using NUnit.Framework;
using Redirxn.TeamKitty.ViewModels;
using Redirxn.TeamKitty.Services.Logic;
using Splat;
using Redirxn.TeamKitty.Models;
using FluentAssertions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Redirxn.TeamKitty.Tests
{
    [TestFixture]
    public partial class StatusViewModelTests : BaseTest
    {
        private const string myEmail = "me@myplace";
        StatusViewModel _vmStatus;

        private void Setup()
        {
            _vmStatus = new StatusViewModel();
            _vmStatus.OnAppearing();
        }

        [TestCase(-5.0, "You still owe $5.00")]
        [TestCase(0, "All paid up")]
        [TestCase(5, "You are ahead $5.00")]
        public async Task CanShowBalancetext(decimal balance, string expected)
        {
            await Prepare(balance);
            Setup();

            _vmStatus.MyBalanceText.Should().Be(expected);
        }

        private async Task Prepare(decimal balance)
        {
            base.Setup();
            var fakeLoginData = new NetworkAuthData
            {
                Email = myEmail,
                Id = myEmail
            };
            await Locator.Current.GetService<IIdentityService>().Init("fakeToken", fakeLoginData);

            var kitty = GetFakeAdminKitty();

            kitty.Ledger.Summary.First().Balance = balance;
            Db.MakeGetKittyReturn(kitty);
            await Locator.Current.GetService<IKittyService>().LoadKitty("anything");

        }

        [Test]
        public async Task CanPayMoney()
        {
            await Prepare(0);
            Dialogs.Make_MoneyInputReturn("100");
            Setup();

            _vmStatus.PayCommand.Execute(null);

            Db.SaveKittyToDbKitty.Ledger.Transactions.Where(t => t.TransactionType == TransactionType.Payment).First().TransactionAmount.Should().Be(100M);
            Db.SaveKittyToDbKitty.Ledger.Summary.First(lsl => lsl.Person.Email == myEmail).Balance.Should().Be(100M);
        }
        [Test]
        public async Task CanSupplyStock()
        {
            await Prepare(0);
            Dialogs.Make_SelectOptionReturn("Case of Item1");
            Setup();

            _vmStatus.ProvideCommand.Execute(null);

            Db.SaveKittyToDbKitty.Ledger.Transactions.Where(t => t.TransactionType == TransactionType.Provision).First().TransactionName.Should().Be("Item1");
            Db.SaveKittyToDbKitty.Ledger.Transactions.Where(t => t.TransactionType == TransactionType.Provision).First().TransactionCount.Should().Be(1);
            Db.SaveKittyToDbKitty.Ledger.Summary.First(lsl => lsl.Person.Email == myEmail).Provisions["Item1"].Should().Be(1);
        }
        [Test]
        public async Task CanRenameMyself()
        {
            await Prepare(0);
            Setup();
            
            const string userName = "HeyItsaMeMario";
            Dialogs.Make_TextInputReturn(userName);

            _vmStatus.ChangeMyNameCommand.Execute(null);

            Db.SaveKittyToDbKitty.Ledger.Summary.FirstOrDefault(ls => ls.Person.DisplayName == userName).Should().NotBeNull();
            Db.SaveKittyToDbKitty.Ledger.Summary.FirstOrDefault(ls => ls.Person.DisplayName == userName).Person.Email.Should().NotBe(userName);
            Db.SaveUserDetailToDbUser.Name.Should().Be(userName);
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
                            StockGrouping="Case"
                        },
                        new StockItem
                        {
                            MainName="Item2",
                            SalePrice=5M,
                            StockGrouping="Vat"
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
                    },
                },
                Id = "me|FakeKitty"
            };
        }
    }
}