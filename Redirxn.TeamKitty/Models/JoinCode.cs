using System;

namespace Redirxn.TeamKitty.Models
{
    public class JoinCode
    {
        public string Code { get; set; }
        public DateTime Expiry { get; set; } = DateTime.Now.AddDays(1);
    }
}