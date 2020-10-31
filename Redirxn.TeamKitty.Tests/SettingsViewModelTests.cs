﻿using NUnit.Framework;
using Redirxn.TeamKitty.ViewModels;
using Redirxn.TeamKitty.Services.Logic;
using Splat;
using Redirxn.TeamKitty.Models;
using FluentAssertions;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System;
using Redirxn.TeamKitty.Views;

namespace Redirxn.TeamKitty.Tests
{
    [TestFixture]
    public class SettingsViewModelTests : BaseTest
    {
        private const string myEmail = "me@myplace";
        private const string TestCode = "MYCODE";

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

            string NewKittyName = await CreateTestKitty();

            AssertKittyIsCreatedCorrectly(NewKittyName);
            AssertIWasMadeAdmin();
        }

        private async Task<string> CreateTestKitty()
        {
            const string NewKittyName = "TestKittyName";
            Dialogs.Make_TextInputReturn(NewKittyName);

            await vmSettings.CreateNewKitty();
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
        public async Task CanJoinAnExistingKitty()
        {
            await SetupAsync();

            const string testId = "IhaveAn|Id";
            Dialogs.Make_TextInputReturn(TestCode);
            Db.MakeGetKittyIdReturnThisId(testId);
            Db.MakeGetKittyReturn(new Kitty { Id = testId }, testId);

            await vmSettings.JoinKitty();

            AssertKittyIsCreatedCorrectly("Id");
        }
        [Test]
        public async Task CanInviteToKitty()
        {
            await SetupAsync();
            string NewKittyName = await CreateTestKitty();
            Db.MakeGetCodesForKittyIdReturn(new List<JoinCode> { new JoinCode { Code = TestCode, Expiry = DateTime.Now.AddHours(1) } });

            await vmSettings.Invite();

            Dialogs.AlertText.Should().NotBeNullOrEmpty();
            Db.DeletedCodes.First(jc => jc.Code == TestCode).Should().NotBeNull();
            Db.JoinCodeThatWasSet.Should().Be(TestCode);
        }
        [Test]
        public async Task CanAddUser()
        {
            await SetupAsync();
            string NewKittyName = await CreateTestKitty();
            const string userName = "NewGuy";
            Dialogs.Make_TextInputReturn(userName);
            Db.MakeGetKittyReturn(Db.SaveKittyToDbKitty);

            await vmSettings.AddUser();

            Db.SaveKittyToDbKitty.Ledger.Summary.FirstOrDefault(ls => ls.Person.DisplayName == userName).Should().NotBeNull();
        }
        [Test]
        public async Task CanRenameMyself()
        {
            await SetupAsync();
            string NewKittyName = await CreateTestKitty();
            const string userName = "HeyItsaMeMario";
            Dialogs.Make_TextInputReturn(userName);
            Db.MakeGetKittyReturn(Db.SaveKittyToDbKitty);

            await vmSettings.ChangeMyName();

            Db.SaveKittyToDbKitty.Ledger.Summary.FirstOrDefault(ls => ls.Person.DisplayName == userName).Should().NotBeNull();
            Db.SaveUserDetailToDbUser.Name.Should().Be(userName);
        }
        [Test]
        public async Task CantRenameMyselfIfNameIsInUse()
        {
            await SetupAsync();
            string NewKittyName = await CreateTestKitty();
            const string userName = "HeyItsaMeMario";
            Dialogs.Make_TextInputReturn(userName);
            Db.MakeGetKittyReturn(Db.SaveKittyToDbKitty);
            await vmSettings.AddUser();

            await vmSettings.ChangeMyName();

            Db.SaveUserDetailToDbUser.Name.Should().NotBe(userName);
        }
    }

}