namespace Redirxn.TeamKitty.Models
{
    public class SessionOrder
    {
        public string PersonId { get; set; }
        public string PersonDisplayName { get; set; }
        public string StockItemName { get; set; }
        public string OptionName { get; set; }
        public bool IsDelivered { get; set; }
    }
}
