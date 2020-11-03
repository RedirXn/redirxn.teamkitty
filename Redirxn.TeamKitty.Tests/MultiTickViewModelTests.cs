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