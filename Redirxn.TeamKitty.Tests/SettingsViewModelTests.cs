using NUnit.Framework;
using Redirxn.TeamKitty.ViewModels;
using Redirxn.TeamKitty.Services.Logic;
using Splat;
using Redirxn.TeamKitty.Models;
using FluentAssertions;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System;

namespace Redirxn.TeamKitty.Tests
{
    [TestFixture]
    public class SettingsViewModelTests : BaseTest
    {
        private const string myEmail = "me@myplace";
        SettingsViewModel vmSettings;

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
        }
        private async Task SetupAsync()
        {
            vmSettings = new SettingsViewModel();
            await vmSettings.Init();
        }
        [Test]
        public async Task KittylessShouldDeactivateNonCreateSettings()
        {
            await SetupAsync();
            vmSettings.KittyExists.Should().BeFalse();            
        }

        [Test]
        public async Task CanCreateANewKitty()
        {
            await SetupAsync();

            const string NewKittyName = "TestKittyName";
            Dialogs.Make_TextInputReturn(NewKittyName);

            await vmSettings.CreateNewKitty();

            AssertKittyIsCreatedCorrectly(NewKittyName);
            AssertIWasMadeAdmin();
        }

        private void AssertIWasMadeAdmin()
        {
            Db.SaveKittyToDbKitty.Administrators.Should().Contain(myEmail);
        }

        private void AssertKittyIsCreatedCorrectly(string NewKittyName)
        {
            Db.SaveKittyToDbKitty.DisplayName.Should().Be(NewKittyName);
            Db.SaveKittyToDbKitty.Ledger.Summary.First(ls => ls.Person.Email == myEmail).Should().NotBeNull();
            Db.SaveUserDetailToDbUser.DefaultKitty.Should().Be(Db.SaveKittyToDbKitty.Id);
            Db.SaveUserDetailToDbUser.Id.Should().Be(myEmail);
            Db.SaveUserDetailToDbUser.KittyNames.Should().Contain(Db.SaveKittyToDbKitty.Id);
        }

        [Test]
        public async Task CanJoinAnExistingKitty()
        {
            await SetupAsync();

            const string testCode = "MYCODE";
            const string testId = "IhaveAn|Id";
            Dialogs.Make_TextInputReturn(testCode);
            Db.MakeGetKittyIdReturnThisId(testId);
            Db.MakeGetKittyReturn(new Kitty { Id = testId }, testId);

            await vmSettings.JoinKitty();

            AssertKittyIsCreatedCorrectly("Id");
        }
        [Test]
        public async Task CanInviteToKitty()
        {
            await SetupAsync();
            const string NewKittyName = "TestKittyName";
            const string jCode = "MYCODE"; 
            Dialogs.Make_TextInputReturn(NewKittyName);            
            Db.MakeGetCodesForKittyIdReturn(new List<JoinCode> { new JoinCode { Code = jCode, Expiry = DateTime.Now.AddHours(1) } });
            await vmSettings.CreateNewKitty();

            await vmSettings.Invite();

            Dialogs.AlertText.Should().NotBeNullOrEmpty();
            Db.DeletedCodes.First(jc => jc.Code == jCode).Should().NotBeNull();
            Db.JoinCodeThatWasSet.Should().Be(jCode);
        }
    }
}