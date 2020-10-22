using System;
using System.Collections.Generic;
using System.Text;

namespace Redirxn.TeamKitty.Models
{
    public class UserInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public IEnumerable<string> KittyNames { get; set; }
        public string DefaultKitty { get; set; }
    }
}
