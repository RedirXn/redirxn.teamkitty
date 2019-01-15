namespace TeamKitty.Forms.Domain
{
    internal class Purchase
    {
        public string Member { get; private set; } 
        public Sku Sku { get; private set; }

        public Purchase(string member, Sku sku)
        {
            this.Member = member;
            this.Sku = sku;
        }
    }
}