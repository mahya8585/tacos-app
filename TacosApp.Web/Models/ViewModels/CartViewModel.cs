using System.Collections.Generic;

namespace TacosApp.Web.Models.ViewModels
{
    public class CartViewModel
    {
        public CartViewModel()
        {
            this.Items = new List<CartItemViewModel>();
        }

        public List<CartItemViewModel> Items { get; set; }

        public decimal Total
        {
            get
            {
                decimal sum = 0;
                foreach (CartItemViewModel item in this.Items)
                {
                    sum += item.SubTotal;
                }
                return sum;
            }
        }

        public int ItemCount
        {
            get
            {
                int count = 0;
                foreach (CartItemViewModel item in this.Items)
                {
                    count += item.Quantity;
                }
                return count;
            }
        }
    }
}
