using NUnit.Framework;
using Redirxn.TeamKitty.ViewModels;
using Redirxn.TeamKitty.Services.Logic;
using Splat;
using Redirxn.TeamKitty.Models;
using System.Threading.Tasks;

namespace Redirxn.TeamKitty.Tests
{
    [TestFixture]
    public class NoKittyTests : BaseTest
    {
        private const string myEmail = "me@myplace";
        private async Task PrepareMinimum()
        {
            var fakeLoginData = new NetworkAuthData
            {
                Email = myEmail,
                Id = myEmail
            };
            await Locator.Current.GetService<IIdentityService>().Init("fakeToken", fakeLoginData);
        }

        [Test]
        public async Task StatusPageLoadsWithoutError()
        {
            await PrepareMinimum();

            var _vm = new StatusViewModel();
            _vm.OnAppearing();
            _vm.LoadTransactionsCommand.Execute(null);
        }

        [Test]
        public async Task KittyPageLoadsWithoutError()
        {
            await PrepareMinimum();

            var _vm = new KittyViewModel();
            _vm.OnAppearing();
            _vm.LoadItemsCommand.Execute(null);
        }
        [Test]
        public async Task MainPageLoadsWithoutError()
        {
            await PrepareMinimum();

            var _vm = new MainViewModel();
            await _vm.Init();
            _vm.LoadItemsCommand.Execute(null);
        }
        [Test]
        public async Task StockPageLoadsWithoutError()
        {
            await PrepareMinimum();

            var _vm = new StockViewModel();
            _vm.OnAppearing();
            _vm.LoadItemsCommand.Execute(null);
        }
        [Test]
        public async Task HistoryPageLoadsWithoutError()
        {
            await PrepareMinimum();

            var _vm = new HistoryViewModel();
            _vm.OnAppearing();
            _vm.LoadTransactionsCommand.Execute(null);
        }

    }

}