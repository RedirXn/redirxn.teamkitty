using System;
using System.Collections.Generic;
using System.Text;

namespace Redirxn.TeamKitty.Models
{
    public class UserInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public IList<string> KittyNames { get; set; } = new List<string>();
        public string DefaultKitty { get; set; }
    }
}
