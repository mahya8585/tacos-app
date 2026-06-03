using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TacosApp.Web.Models.Domain
{
    public class OrderItemTopping
    {
        [Key]
        public int OrderItemToppingId { get; set; }

        [ForeignKey("OrderItem")]
        public int OrderItemId { get; set; }

        [ForeignKey("Topping")]
        public int ToppingId { get; set; }

        public decimal UnitPrice { get; set; }

        public virtual OrderItem OrderItem { get; set; }
        public virtual Topping Topping { get; set; }
    }
}
