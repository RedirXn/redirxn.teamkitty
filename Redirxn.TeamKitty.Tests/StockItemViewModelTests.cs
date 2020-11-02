using NUnit.Framework;
using Redirxn.TeamKitty.ViewModels;
using Redirxn.TeamKitty.Services.Logic;
using Splat;
using Redirxn.TeamKitty.Models;
using FluentAssertions;
using System.Linq;

namespace Redirxn.TeamKitty.Tests
{
    [TestFixture]
    public class StockItemViewModelTests : BaseTest
    {
        private const string myEmail = "me@myplace";
        private StockItemViewModel _vmStockItem;

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

            _vmStockItem = new StockItemViewModel();
            
        }
        [Test]
        public void CanDeleteStockItem()
        {
            _vmStockItem.MainName = "Item2";
            _vmStockItem.OnDeleteStockCommand.Execute(null);

            Db.SaveKittyToDbKitty.KittyConfig.StockItems.FirstOrDefault(si => si.MainName == "Item2").Should().BeNull();
            Routes.WasGoBackCalled().Should().BeTrue();
        }
        [Test]
        public void CanSaveStockItem()
        {
            var si = GetFakeAdminKitty().KittyConfig.StockItems.First();
            _vmStockItem.FromMainName = si.MainName;
            const decimal newPrice = 55M;
            _vmStockItem.Price = newPrice;

            _vmStockItem.SaveItemCommand.Execute(null);

            Db.SaveKittyToDbKitty.KittyConfig.StockItems.FirstOrDefault(i => i.MainName == si.MainName).SalePrice.Should().Be(newPrice);
            Routes.WasGoBackCalled().Should().BeTrue();
        }
        [Test]
        public void CanAddNewStockItem()
        {
            const string newName = "New Item";
            _vmStockItem.MainName = newName;

            _vmStockItem.SaveItemCommand.Execute(null);

            Db.SaveKittyToDbKitty.KittyConfig.StockItems.FirstOrDefault(i => i.MainName == newName).Should().NotBeNull();
            Routes.WasGoBackCalled().Should().BeTrue();
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