using System.ComponentModel.DataAnnotations.Schema;

namespace TacosApp.Admin.Models;

public class OrderItem
{
    public int OrderItemId { get; set; }

    public int OrderId { get; set; }

    public int MenuId { get; set; }

    public int Quantity { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; }

    public Order? Order { get; set; }

    public Menu? Menu { get; set; }

    public ICollection<OrderItemTopping> Toppings { get; init; } = [];
}
