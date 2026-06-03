using System.ComponentModel.DataAnnotations.Schema;

namespace TacosApp.Admin.Models;

public class OrderItemTopping
{
    public int OrderItemToppingId { get; set; }

    public int OrderItemId { get; set; }

    public int ToppingId { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; }

    public OrderItem? OrderItem { get; set; }

    public Topping? Topping { get; set; }
}
