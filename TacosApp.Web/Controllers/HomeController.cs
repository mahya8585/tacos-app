using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using TacosApp.Web.Data;
using TacosApp.Web.Models.Domain;
using TacosApp.Web.Models.ViewModels;

namespace TacosApp.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly TacosDbContext _db;

        public HomeController()
        {
            _db = new TacosDbContext();
        }

        public ActionResult Index()
        {
            IQueryable<Menu> menusQuery = from m in _db.Menus
                                          where m.IsAvailable
                                          orderby m.DisplayOrder
                                          select m;
            List<Menu> menus = menusQuery.ToList();

            IQueryable<Topping> toppingsQuery = from t in _db.Toppings
                                                where t.IsAvailable
                                                orderby t.DisplayOrder
                                                select t;
            List<Topping> toppings = toppingsQuery.ToList();

            List<MenuItemViewModel> menuVms = new List<MenuItemViewModel>();
            foreach (Menu m in menus)
            {
                MenuItemViewModel mvm = new MenuItemViewModel
                {
                    MenuId = m.MenuId,
                    Name = m.Name,
                    Description = m.Description,
                    Price = m.Price,
                    ImageUrl = m.ImageUrl
                };
                menuVms.Add(mvm);
            }

            List<ToppingItemViewModel> toppingVms = new List<ToppingItemViewModel>();
            foreach (Topping t in toppings)
            {
                ToppingItemViewModel tvm = new ToppingItemViewModel
                {
                    ToppingId = t.ToppingId,
                    Name = t.Name,
                    Price = t.Price
                };
                toppingVms.Add(tvm);
            }

            MenuIndexViewModel vm = new MenuIndexViewModel
            {
                Menus = menuVms,
                Toppings = toppingVms
            };

            return View(vm);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
