using System.Collections.Generic;

namespace TeamKitty.Forms.Domain
{
    internal class MemberProduct
    {
        public string Member { get; internal set; }
        public Product Product { get; internal set; }
        public List<Purchase> Sales { get; internal set; }
    }
}