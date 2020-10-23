using Redirxn.TeamKitty.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Redirxn.TeamKitty.Services.Gateway
{
    public interface IDataStore
    {
        void Init(string activeToken);
        Task<UserInfo> GetUserDetail(string email);
        Task<UserInfo> CreateNewKitty(NetworkAuthData loginData, UserInfo userDetail, string newKittyName);
    }
}
