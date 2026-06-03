using System.Collections.Generic;

namespace TacosApp.Web.Models.ViewModels
{
    public class MenuIndexViewModel
    {
        public List<MenuItemViewModel> Menus { get; set; }
        public List<ToppingItemViewModel> Toppings { get; set; }
    }

    public class MenuItemViewModel
    {
        public int MenuId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string ImageUrl { get; set; }
    }

    public class ToppingItemViewModel
    {
        public int ToppingId { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
    }
}
