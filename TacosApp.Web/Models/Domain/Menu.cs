using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TacosApp.Web.Models.Domain
{
    public class Menu
    {
        [Key]
        public int MenuId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        public decimal Price { get; set; }

        [StringLength(500)]
        public string ImageUrl { get; set; }

        public bool IsAvailable { get; set; }

        public int DisplayOrder { get; set; }

        public virtual ICollection<OrderItem> OrderItems { get; set; }
    }
}
