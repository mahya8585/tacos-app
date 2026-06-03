using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TacosApp.Web.Models.Domain
{
    public class OrderItem
    {
        [Key]
        public int OrderItemId { get; set; }

        [ForeignKey("Order")]
        public int OrderId { get; set; }

        [ForeignKey("Menu")]
        public int MenuId { get; set; }

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public virtual Order Order { get; set; }
        public virtual Menu Menu { get; set; }
        public virtual ICollection<OrderItemTopping> Toppings { get; set; }
    }
}
