using System.Collections.Generic;

namespace Redirxn.TeamKitty.Models
{
    public class Session
    {
        public bool IsStarted { get; set; }
        public string PersonTakingOrders { get; set; }
        public IList<SessionOrder> Orders { get; set; } = new List<SessionOrder>();
        public Dictionary<string, string> OrderOptions { get; set; } = new Dictionary<string, string>();
    }
}
