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
using Redirxn.TeamKitty.Views;

namespace Redirxn.TeamKitty.Tests
{
    [TestFixture]
    public class SettingsViewModelTests : BaseTest
    {
        private const string myEmail = "me@myplace";
        private const string TestCode = "MYCODE";

        //SettingsViewModel vmSettings;

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
            //vmSettings = new SettingsViewModel();
            //await vmSettings.Init();
        }
        
    }

}