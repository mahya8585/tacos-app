using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TacosApp.Web.Models.Domain
{
    public class Topping
    {
        [Key]
        public int ToppingId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        public decimal Price { get; set; }

        public bool IsAvailable { get; set; }

        public int DisplayOrder { get; set; }

        public virtual ICollection<OrderItemTopping> OrderItemToppings { get; set; }
    }
}
