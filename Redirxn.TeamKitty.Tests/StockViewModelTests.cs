using NUnit.Framework;
using Redirxn.TeamKitty.ViewModels;
using Redirxn.TeamKitty.Services.Logic;
using Splat;
using Redirxn.TeamKitty.Models;
using FluentAssertions;

namespace Redirxn.TeamKitty.Tests
{
    [TestFixture]
    public class StockViewModelTests : BaseTest
    {
        private const string myEmail = "me@myplace";
        private StockViewModel _vmStock;

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

            _vmStock = new StockViewModel();
            _vmStock.LoadItemsCommand.Execute(null);
        }
        [Test]
        public void CanDetermineAdmin()
        {
            _vmStock.IsAdmin.Should().BeTrue();
        }

        [Test]
        public void CanLoadAllStockItems()
        {
            _vmStock.Items.Count.Should().Be(2);
        }

        [Test]
        public void CanNavigateOnSelectItem()
        {
            var si = new StockItem
            {
                MainName = "Item1",
                SalePrice = 2.5M
            };

            _vmStock.ItemTapped.Execute(si);

            Routes.WasNavigatedTo("StockItemPage?FromMainName=Item1").Should().BeTrue();
        }

        private Kitty GetFakeAdminKitty()
        {
            return new Kitty
            {
                Administrators = new []{ myEmail},
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