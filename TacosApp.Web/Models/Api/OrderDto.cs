using System;
using System.Collections.Generic;

namespace TacosApp.Web.Models.Api
{
    /// <summary>管理アプリに返す注文情報DTO</summary>
    public class OrderDto
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; }
        public string CustomerName { get; set; }
        public string Phone { get; set; }
        public string DeliveryAddress { get; set; }
        public string DeliveryNote { get; set; }
        public decimal TotalAmount { get; set; }
        public int Status { get; set; }
        public string StatusLabel { get; set; }
        public DateTime OrderedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<OrderItemDto> Items { get; set; }
    }

    public class OrderItemDto
    {
        public int OrderItemId { get; set; }
        public string MenuName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public List<OrderItemToppingDto> Toppings { get; set; }
    }

    public class OrderItemToppingDto
    {
        public string ToppingName { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
