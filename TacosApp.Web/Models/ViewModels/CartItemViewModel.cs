using System.Collections.Generic;

namespace TacosApp.Web.Models.ViewModels
{
    /// <summary>カート内の1明細（タコス1種類 + トッピング）</summary>
    public class CartItemViewModel
    {
        public CartItemViewModel()
        {
            this.Toppings = new List<CartToppingViewModel>();
        }

        public string ItemKey { get; set; }
        public int MenuId { get; set; }
        public string MenuName { get; set; }
        public decimal MenuPrice { get; set; }
        public int Quantity { get; set; }
        public List<CartToppingViewModel> Toppings { get; set; }

        public decimal SubTotal
        {
            get { return (this.MenuPrice + this.ToppingTotal) * this.Quantity; }
        }

        public decimal ToppingTotal
        {
            get
            {
                decimal sum = 0;
                foreach (CartToppingViewModel t in this.Toppings)
                {
                    sum += t.Price;
                }
                return sum;
            }
        }
    }

    public class CartToppingViewModel
    {
        public int ToppingId { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
    }
}
