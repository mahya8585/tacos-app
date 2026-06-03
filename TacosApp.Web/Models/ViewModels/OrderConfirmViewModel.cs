using System.Collections.Generic;

namespace TacosApp.Web.Models.ViewModels
{
    /// <summary>注文確認ページ用ViewModel（セッションのカート + 配達情報の要約表示）</summary>
    public class OrderConfirmViewModel
    {
        public OrderConfirmViewModel()
        {
            this.Items = new List<CartItemViewModel>();
        }

        public string CustomerName { get; set; }
        public string Phone { get; set; }
        public string DeliveryAddress { get; set; }
        public string DeliveryNote { get; set; }
        public List<CartItemViewModel> Items { get; set; }
        public decimal Total { get; set; }
    }
}
