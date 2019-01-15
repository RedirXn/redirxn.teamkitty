using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace TeamKitty.Forms.Domain
{
    public class Team
    {
        public string Name { get; internal set; }

        private List<Sku> _products = new List<Sku>();
        private List<string> _members = new List<string>();
        private Ledger _ledger = new Ledger();

        internal void AddSku(string productName, decimal price)
        {
            Product prod;
            if (Enum.TryParse(productName.Replace(" ", "").Replace("-", ""), out prod))
            {
                _products.Add(new Sku(prod, price));
            }
        }
        
        internal List<Sku> GetSkus()
        {
            return _products;
        }

        internal void AddMember(string member)
        {
            _members.Add(member);
        }

        internal void PurchaseProduct(string loggedInUser, string product)
        {
            Product prod;
            if (Enum.TryParse(product, out prod))
            {
                _ledger.PurchaseProduct(loggedInUser, _products.Single(p => p.Product == prod));
            }
        }

        public IEnumerable GetDisplay()
        {
            return _ledger.GetDisplay();
        }

        public BindingList<MemberSummary> MemberRows
        {
            get
            {                
                return _ledger.GetDisplay(); ;
            }
        }

    }
}