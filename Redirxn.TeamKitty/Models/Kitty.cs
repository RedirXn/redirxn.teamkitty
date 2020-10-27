namespace Redirxn.TeamKitty.Models
{
    public class Kitty
    {
        public string Id { get; set; }
        public KittyConfig KittyConfig { get; set; } = new KittyConfig();
        public Ledger Ledger { get; set; } = new Ledger();
        public string DisplayName { get { return Id?.Split('|')[1]; } }
    }
}
