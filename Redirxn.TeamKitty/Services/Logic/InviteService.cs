using Redirxn.TeamKitty.Models;
using Redirxn.TeamKitty.Services.Gateway;
using Splat;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Redirxn.TeamKitty.Services.Logic
{
    public class InviteService : IInviteService
    {
        IJoinCodeDataStore _dataStore;

        public InviteService(IJoinCodeDataStore dataStore = null)
        {
            _dataStore = dataStore ?? Locator.Current.GetService<IJoinCodeDataStore>();
        }
        
        public async Task<string> GetJoinCode(string kittyId)
        {
            List<JoinCode> codes = await _dataStore.GetCodesByKittyId(kittyId);
            string keepCode = string.Empty;
            foreach (var c in codes)
            {
                await _dataStore.DeleteCode(c);
                keepCode = c.Code;
            }
            return await _dataStore.SetNewJoinCode(kittyId, keepCode ?? CreateJoinCode());
        }

        public async Task<string> GetKittyIdWithCode(string joinCode)
        {            
            return await _dataStore.GetKittyIdWithCode(joinCode);
        }
        private string CreateJoinCode()
        {
            var chars = "ABCDEFGHIJKLMNPQRSTUVWXYZ23456789";
            var stringChars = new char[6];
            var random = new Random();

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            return new string(stringChars);
        }
    }
}
