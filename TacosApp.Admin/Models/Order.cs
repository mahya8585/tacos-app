using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TacosApp.Admin.Models;

public class Order
{
    public int OrderId { get; set; }

    [Required, StringLength(30)]
    public required string OrderNumber { get; set; }

    [Required, StringLength(100)]
    public required string CustomerName { get; set; }

    [Required, StringLength(20)]
    public required string Phone { get; set; }

    [Required, StringLength(500)]
    public required string DeliveryAddress { get; set; }

    [StringLength(500)]
    public string? DeliveryNote { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }

    public OrderStatus Status { get; set; }

    public DateTime OrderedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public ICollection<OrderItem> Items { get; init; } = [];
}
