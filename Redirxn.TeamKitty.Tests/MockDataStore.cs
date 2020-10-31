using Redirxn.TeamKitty.Models;
using Redirxn.TeamKitty.Services.Gateway;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Redirxn.TeamKitty.Tests
{
    public class MockDataStore : IKittyDataStore, IUserDataStore, IJoinCodeDataStore
    {
        public Kitty SaveKittyToDbKitty { get; private set; }
        public UserInfo SaveUserDetailToDbUser { get; private set; }

        private Kitty _kittyToReturn;
        private Kitty _kittyToConditionallyReturn;
        private string _kittyReturnCondition;
        private string _kittyReturnId;
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
            if (kittyId == _kittyReturnCondition)
            {
                return _kittyToConditionallyReturn;
            }
            else
            {
                return _kittyToReturn;
            }
        }

        public async Task<string> GetKittyIdWithCode(string joinCode)
        {
            return _kittyReturnId;
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
            SaveKittyToDbKitty = kitty;            
        }

        public async Task SaveUserDetailToDb(UserInfo userDetail)
        {
            SaveUserDetailToDbUser = userDetail;            
        }

        public Task<string> SetNewJoinCode(string kittyId, string code)
        {
            throw new System.NotImplementedException();
        }

        internal void MakeGetKittyReturn(Kitty kitty, string withThisId = null)
        {
            if (withThisId == null)
            {
                _kittyToReturn = kitty;
                return;
            }
            _kittyReturnCondition = withThisId;
            _kittyToConditionallyReturn = kitty;
        }
        internal void MakeGetKittyIdReturnThisId(string id)
        {
            _kittyReturnId = id;
        }
    }
}