using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TacosApp.Admin.Models;

public class Topping
{
    public int ToppingId { get; set; }

    [Required, StringLength(100)]
    public required string Name { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    public bool IsAvailable { get; set; } = true;

    public int DisplayOrder { get; set; }

    public ICollection<OrderItemTopping> OrderItemToppings { get; init; } = [];
}
