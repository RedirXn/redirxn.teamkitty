﻿using System.Collections.Generic;

namespace Redirxn.TeamKitty.Models
{
    public class Kitty
    {
        public string Id { get; set; }
        public KittyConfig KittyConfig { get; set; } = new KittyConfig();
        public Ledger Ledger { get; set; } = new Ledger();
        public string DisplayName { get { return Id?.Split('|')[1] + ((KittyConfig.Locked) ? " (LOCKED) " : string.Empty); } }
        public IEnumerable<string> Administrators { get; set; } = new string[0];
        public Session Session { get; set; } = new Session();
    }
}
