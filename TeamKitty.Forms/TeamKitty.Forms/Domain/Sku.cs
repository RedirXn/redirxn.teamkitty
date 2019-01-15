namespace TeamKitty.Forms.Domain
{
    public class Sku
    {
        internal Sku(Product product, decimal price)
        {
            Product = product;
            Price = price;
        }

        public Product Product { get; set; }
        public decimal Price { get; set; }
    }
}