using Redirxn.TeamKitty.Models;
using Redirxn.TeamKitty.Services.Gateway;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Redirxn.TeamKitty.Tests
{
    internal class MockDataStore : IKittyDataStore, IUserDataStore, IJoinCodeDataStore
    {
        public bool SaveKittyToDbCalled { get; private set; } 
        public MockDataStore()
        {
        }

        public Task DeleteCode(JoinCode c)
        {
            throw new System.NotImplementedException();
        }

        public Task<List<JoinCode>> GetCodesByKittyId(string kittyId)
        {
            throw new System.NotImplementedException();
        }

        public async Task<Kitty> GetKitty(string kittyId)
        {
            return null;
        }

        public Task<string> GetKittyIdWithCode(string joinCode)
        {
            throw new System.NotImplementedException();
        }

        public async Task<UserInfo> GetUserDetail(string email)
        {
            return new UserInfo();
        }

        public void Init(string activeToken)
        {
            
        }

        public async Task SaveKittyToDb(Kitty kitty)
        {
            SaveKittyToDbCalled = true;
            return;
        }

        public Task SaveUserDetailToDb(UserInfo userDetail)
        {
            throw new System.NotImplementedException();
        }

        public Task<string> SetNewJoinCode(string kittyId, string code)
        {
            throw new System.NotImplementedException();
        }
    }
}