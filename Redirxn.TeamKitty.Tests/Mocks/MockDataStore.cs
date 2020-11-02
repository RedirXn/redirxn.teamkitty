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
        public List<JoinCode> DeletedCodes { get; private set; }
        public string JoinCodeThatWasSet { get; private set; }
        private Kitty _kittyToReturn;
        private Kitty _kittyToConditionallyReturn;
        private string _kittyReturnCondition = "";
        private string _kittyReturnId;
        private List<JoinCode> _codes;
        
        public MockDataStore()
        {
        }

        public async Task DeleteCode(JoinCode c)
        {
            if (DeletedCodes == null)
            {
                DeletedCodes = new List<JoinCode>();
            }
            DeletedCodes.Add(c);
        }

        public void MakeGetCodesForKittyIdReturn(List<JoinCode> codes)
        {
            _codes = codes;
        }
        public async Task<List<JoinCode>> GetCodesByKittyId(string kittyId)
        {
            return _codes;
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

        public async Task<string> SetNewJoinCode(string kittyId, string code)
        {
            JoinCodeThatWasSet = code;
            return code;
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