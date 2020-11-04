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

        [TestCase(-5.0, "You owe $5.00")]
        [TestCase(0, "You Owe Nothing")]
        [TestCase(5, "You are ahead $5.00")]
        public void CanShowBalancetext(decimal balance, string expected)
        {
            var fakeLoginData = new NetworkAuthData { Email = myEmail, Id = myEmail };
            Locator.Current.GetService<IIdentityService>().Init("fakeToken", fakeLoginData);
            var kitty = GetFakeAdminKitty();
            kitty.Ledger.Summary.First().Balance = balance;
            Db.MakeGetKittyReturn(kitty);
            Locator.Current.GetService<IKittyService>().LoadKitty("anything");

            _vmStatus = new StatusViewModel();

            _vmStatus.MyBalanceText.Should().Be(expected);
        }
        [Test]
        public void CanPayMoney()
        {
            var fakeLoginData = new NetworkAuthData { Email = myEmail, Id = myEmail };
            Locator.Current.GetService<IIdentityService>().Init("fakeToken", fakeLoginData);
            var kitty = GetFakeAdminKitty();            
            Db.MakeGetKittyReturn(kitty);
            Locator.Current.GetService<IKittyService>().LoadKitty("anything");
            _vmStatus = new StatusViewModel();
            Dialogs.Make_MoneyInputReturn("100");

            _vmStatus.PayCommand.Execute(null);

            Db.SaveKittyToDbKitty.Ledger.Transactions.Where(t => t.TransactionType == TransactionType.Payment).First().TransactionAmount.Should().Be(100M);
            Db.SaveKittyToDbKitty.Ledger.Summary.First(lsl => lsl.Person.Email == myEmail).Balance.Should().Be(100M);
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
                    },
                },
                Id = "me|FakeKitty"
            };
        }
    }
}