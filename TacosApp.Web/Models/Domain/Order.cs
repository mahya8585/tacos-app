using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TacosApp.Web.Models.Domain
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }

        /// <summary>注文番号（例: TACOS-240101-000001）</summary>
        [Required]
        [StringLength(30)]
        public string OrderNumber { get; set; }

        [Required]
        [StringLength(100)]
        public string CustomerName { get; set; }

        [Required]
        [StringLength(20)]
        public string Phone { get; set; }

        [Required]
        [StringLength(500)]
        public string DeliveryAddress { get; set; }

        [StringLength(500)]
        public string DeliveryNote { get; set; }

        public decimal TotalAmount { get; set; }

        public OrderStatus Status { get; set; }

        public DateTime OrderedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public virtual ICollection<OrderItem> Items { get; set; }
    }
}
