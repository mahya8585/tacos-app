using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TacosApp.Admin.Models;

public class Menu
{
    public int MenuId { get; set; }

    [Required, StringLength(100)]
    public required string Name { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    [StringLength(500)]
    public string? ImageUrl { get; set; }

    public bool IsAvailable { get; set; } = true;

    public int DisplayOrder { get; set; }

    public ICollection<OrderItem> OrderItems { get; init; } = [];
}
