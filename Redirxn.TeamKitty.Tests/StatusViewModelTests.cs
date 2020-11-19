using NUnit.Framework;
using Redirxn.TeamKitty.ViewModels;
using Redirxn.TeamKitty.Services.Logic;
using Splat;
using Redirxn.TeamKitty.Models;
using FluentAssertions;
using System.Collections.Generic;
using System.Linq;

namespace Redirxn.TeamKitty.Tests
{
    [TestFixture]
    public class StatusViewModelTests : BaseTest
    {
        private const string myEmail = "me@myplace";
        StatusViewModel _vmStatus;

        [TestCase(-5.0, "You still owe $5.00")]
        [TestCase(0, "All paid up")]
        [TestCase(5, "You are ahead $5.00")]
        public void CanShowBalancetext(decimal balance, string expected)
        {
            Prepare(balance);
            _vmStatus = new StatusViewModel();

            _vmStatus.OnAppearing();

            _vmStatus.MyBalanceText.Should().Be(expected);
        }

        private void Prepare(decimal balance)
        {
            var fakeLoginData = new NetworkAuthData { Email = myEmail, Id = myEmail };
            Locator.Current.GetService<IIdentityService>().Init("fakeToken", fakeLoginData);
            var kitty = GetFakeAdminKitty();
            kitty.Ledger.Summary.First().Balance = balance;
            Db.MakeGetKittyReturn(kitty);
            Locator.Current.GetService<IKittyService>().LoadKitty("anything");
        }

        [Test]
        public void CanPayMoney()
        {
            Prepare(0);
            Dialogs.Make_MoneyInputReturn("100");
            _vmStatus = new StatusViewModel();
            _vmStatus.OnAppearing();

            _vmStatus.PayCommand.Execute(null);

            Db.SaveKittyToDbKitty.Ledger.Transactions.Where(t => t.TransactionType == TransactionType.Payment).First().TransactionAmount.Should().Be(100M);
            Db.SaveKittyToDbKitty.Ledger.Summary.First(lsl => lsl.Person.Email == myEmail).Balance.Should().Be(100M);
        }
        [Test]
        public void CanSupplyStock()
        {
            Prepare(0);
            Dialogs.Make_SelectOptionReturn("Case of Item1");
            _vmStatus = new StatusViewModel();
            _vmStatus.OnAppearing();

            _vmStatus.ProvideCommand.Execute(null);

            Db.SaveKittyToDbKitty.Ledger.Transactions.Where(t => t.TransactionType == TransactionType.Provision).First().TransactionName.Should().Be("Case of Item1");
            Db.SaveKittyToDbKitty.Ledger.Transactions.Where(t => t.TransactionType == TransactionType.Provision).First().TransactionCount.Should().Be(1);
            Db.SaveKittyToDbKitty.Ledger.Summary.First(lsl => lsl.Person.Email == myEmail).Provisions["Case of Item1"].Should().Be(1);
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