using Redirxn.TeamKitty.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Redirxn.TeamKitty.Services.Identity
{
    public class IdentityService : IIdentityService
    {
        public bool IsUserLoggedIn { get; set; } = false;
        public NetworkAuthData LoginData { get; set; }
    }
}
