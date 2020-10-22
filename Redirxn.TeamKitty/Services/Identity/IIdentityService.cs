using Redirxn.TeamKitty.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Redirxn.TeamKitty.Services.Identity
{
    public interface IIdentityService
    {
        bool IsUserLoggedIn { get; set; }
        NetworkAuthData LoginData { get; set; }
    }
}
