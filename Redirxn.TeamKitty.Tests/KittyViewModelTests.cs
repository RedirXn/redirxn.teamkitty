using NUnit.Framework;
using Redirxn.TeamKitty.ViewModels;
using Redirxn.TeamKitty.Services.Logic;
using Splat;
using Redirxn.TeamKitty.Models;
using FluentAssertions;
using System.Linq;
using System.Collections.Generic;
using System;

namespace Redirxn.TeamKitty.Tests
{
    [TestFixture]
    public class KittyViewModelTests : BaseTest
    {
        private const string myEmail = "me@myplace";
        private const string TestCode = "MYCODE";

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

            Routes.WasNavigatedTo(@"StatusPage?FromMember=me@myplace").Should().BeTrue();
        }
        [Test]
        public void CanAdjustBalance()
        {
            var fakeLoginData = new NetworkAuthData { Email = myEmail, Id = myEmail };
            Locator.Current.GetService<IIdentityService>().Init("fakeToken", fakeLoginData);
            var kitty = GetFakeAdminKitty();
            kitty.Ledger.Summary.First().Balance = 0;
            Db.MakeGetKittyReturn(kitty);
            Locator.Current.GetService<IKittyService>().LoadKitty("anything");

            Dialogs.Make_MoneyInputReturn("100");
            _vmKitty = new KittyViewModel();
            _vmKitty.OnAppearing();

            _vmKitty.AdjustCommand.Execute(null);

            Db.SaveKittyToDbKitty.Ledger.Transactions.Where(t => t.TransactionType == TransactionType.Adjustment).First().TransactionAmount.Should().Be(100M);
            Locator.Current.GetService<IKittyService>().GetKittyBalance().Should().Be("100.00");
        }

        [Test]
        public void CanCreateANewKitty()
        {
            string NewKittyName = CreateTestKitty();

            AssertKittyIsCreatedCorrectly(NewKittyName);
            AssertIWasMadeAdmin();
        }

        private string CreateTestKitty()
        {
            const string NewKittyName = "TestKittyName";
            Dialogs.Make_TextInputReturn(NewKittyName);

            _vmKitty.CreateKittyCommand.Execute(null);
            return NewKittyName;
        }

        private void AssertIWasMadeAdmin()
        {
            Db.SaveKittyToDbKitty.Administrators.Should().Contain(myEmail);
        }

        private void AssertKittyIsCreatedCorrectly(string NewKittyName)
        {
            Db.SaveKittyToDbKitty.DisplayName.Should().Be(NewKittyName);
            Db.SaveKittyToDbKitty.Ledger.Summary.FirstOrDefault(ls => ls.Person.Email == myEmail).Should().NotBeNull();
            Db.SaveUserDetailToDbUser.DefaultKitty.Should().Be(Db.SaveKittyToDbKitty.Id);
            Db.SaveUserDetailToDbUser.Id.Should().Be(myEmail);
            Db.SaveUserDetailToDbUser.KittyNames.Should().Contain(Db.SaveKittyToDbKitty.Id);
        }

        [Test]
        public void CanJoinAnExistingKitty()
        {

            const string testId = "IhaveAn|Id";
            Dialogs.Make_TextInputReturn(TestCode);
            Db.MakeGetKittyIdReturnThisId(testId);
            Db.MakeGetKittyReturn(new Kitty { Id = testId }, testId);

            _vmKitty.JoinKittyCommand.Execute(null);

            AssertKittyIsCreatedCorrectly("Id");
        }
        [Test]
        public void CanInviteToKitty()
        {

            string NewKittyName = CreateTestKitty();
            Db.MakeGetCodesForKittyIdReturn(new List<JoinCode> { new JoinCode { Code = TestCode, Expiry = DateTime.Now.AddHours(1) } });

            _vmKitty.InviteCommand.Execute(null);

            Dialogs.AlertText.Should().NotBeNullOrEmpty();
            Db.DeletedCodes.First(jc => jc.Code == TestCode).Should().NotBeNull();
            Db.JoinCodeThatWasSet.Should().Be(TestCode);
        }
        [Test]
        public void CanAddUser()
        {            
            string NewKittyName = CreateTestKitty();
            const string userName = "NewGuy";
            Dialogs.Make_TextInputReturn(userName);
            Db.MakeGetKittyReturn(Db.SaveKittyToDbKitty);

            _vmKitty.AddUserCommand.Execute(null);

            Db.SaveKittyToDbKitty.Ledger.Summary.FirstOrDefault(ls => ls.Person.DisplayName == userName).Should().NotBeNull();
            Db.SaveKittyToDbKitty.Ledger.Summary.FirstOrDefault(ls => ls.Person.Email == userName).Should().NotBeNull();
        }
        [Test]
        public void CanMakeSomeoneAdmin()
        {
            string NewKittyName = CreateTestKitty();
            const string userEmail = "AppUser email";
            Db.SaveKittyToDbKitty.Ledger.Summary.Add(new LedgerSummaryLine { Person = new Member { DisplayName = "App User", Email = userEmail } });
            Dialogs.Make_SelectOptionReturn("App User");
            Db.MakeGetKittyReturn(Db.SaveKittyToDbKitty);

            _vmKitty.AssignAdminCommand.Execute(null);

            Db.SaveKittyToDbKitty.Administrators.Should().Contain(userEmail);
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