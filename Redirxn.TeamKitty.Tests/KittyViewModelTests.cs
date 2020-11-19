using NUnit.Framework;
using Redirxn.TeamKitty.ViewModels;
using Redirxn.TeamKitty.Services.Logic;
using Splat;
using Redirxn.TeamKitty.Models;
using FluentAssertions;

namespace Redirxn.TeamKitty.Tests
{
    [TestFixture]
    public class KittyViewModelTests : BaseTest
    {
        private const string myEmail = "me@myplace";
        private KittyViewModel _vmKitty;

        [SetUp]
        public override void Setup()
        {
            base.Setup();

            var fakeLoginData = new NetworkAuthData
            {
                Email = myEmail,
                Id = myEmail
            };
            Locator.Current.GetService<IIdentityService>().Init("fakeToken", fakeLoginData);

            var fakeKitty = GetFakeAdminKitty();
            Db.MakeGetKittyReturn(fakeKitty);
            Locator.Current.GetService<IKittyService>().LoadKitty("anything");

            _vmKitty = new KittyViewModel();
            _vmKitty.LoadItemsCommand.Execute(null);
        }

        [Test]
        public void CanNavigateOnSelectItem()
        {
            var lsl = new LedgerSummaryLine
            {
                Person = new Member { DisplayName="Test", Email=myEmail},                
            };

            _vmKitty.ItemTapped.Execute(lsl);

            Routes.WasNavigatedTo(@"///StatusPage?FromMember=me@myplace").Should().BeTrue();
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
                }
            };
        }
    }
}