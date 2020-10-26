using Redirxn.TeamKitty.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Redirxn.TeamKitty.Services.Gateway
{
    public interface IKittyService
    {
        Kitty Kitty { get; set; }
    }
}
