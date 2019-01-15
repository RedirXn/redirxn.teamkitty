using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace TeamKitty.Forms.Domain
{
    internal class Ledger
    {
        List<Purchase> _purchases = new List<Purchase>();

        internal void PurchaseProduct(string member, Sku sku)
        {
            _purchases.Add(new Purchase(member, sku));
        }

        internal BindingList<MemberSummary> GetDisplay()
        {
            var summary = new BindingList<MemberSummary>();

            var mps = _purchases?.GroupBy(p => new { p.Member, p.Sku.Product })
               .Select(ms => new MemberProduct
               {
                   Member = ms.Key.Member,
                   Product = ms.Key.Product,
                   Sales = ms.ToList(),
               });

            foreach (var mp in mps)
            {
                var memSummary = summary.FirstOrDefault(s => s.MemberName == mp.Member);
                if (memSummary is null)
                {
                    memSummary = new MemberSummary { MemberName = mp.Member };
                    summary.Add(memSummary);
                }
                switch (mp.Product)
                {
                    case Product.Beer:
                        memSummary.BeerTotal = mp.Sales.Sum(s => s.Sku.Price);
                        break;
                    case Product.PreMix:
                        memSummary.PreMixTotal = mp.Sales.Sum(s => s.Sku.Price);
                        break;
                    case Product.SoftDrink:
                        memSummary.SoftDrinkTotal = mp.Sales.Sum(s => s.Sku.Price);
                        break;
                }
            }

            if (summary.Count == 0)
            {
                summary.Add(
                    //new MemberSummary
                    //{
                    //    MemberName = "blah",
                    //    BeerTotal = 0,
                    //    SoftDrinkTotal = 0,
                    //    PreMixTotal = 0,
                    //}
                    new MemberSummary()
                    );
            }
            return summary;

        }
    }
}