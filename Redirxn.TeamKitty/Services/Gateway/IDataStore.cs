﻿using Redirxn.TeamKitty.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Redirxn.TeamKitty.Services.Gateway
{
    public interface IDataStore
    {
        void Init(string activeToken);
        Task<UserInfo> GetUserDetail(string email);
        Task<Kitty> CreateNewKitty(NetworkAuthData loginData, UserInfo userDetail, string newKittyName);        
        Task<Kitty> SaveStockItem(string kittyId, StockItem stockItem);
        Task<Kitty> GetKitty(string kittyId);
        Task<Kitty> DeleteStockItem(string kittyId, string mainName);
        Task<string> SetNewJoinCode(string kittyId);
        Task<string> ResetJoinCode(string kittyId);
        Task<Kitty> JoinKittyWithCode(NetworkAuthData loginData, UserInfo userDetail, string joinCode);
    }
}
