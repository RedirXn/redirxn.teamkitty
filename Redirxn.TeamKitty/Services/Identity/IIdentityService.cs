﻿using Redirxn.TeamKitty.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Redirxn.TeamKitty.Services.Identity
{
    public interface IIdentityService
    {
        bool IsUserLoggedIn { get; } // TODO: CHange this to use App property
        NetworkAuthData LoginData { get; set; }
        UserInfo UserDetail { get; set; }
        Task Init(string activeToken, NetworkAuthData socialLoginData);
        Task ReloadUserDetail();
    }
}
