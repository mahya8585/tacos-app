using System.Collections.Generic;
using System.Web.Mvc;
using TacosApp.Web.Data;
using TacosApp.Web.Models.ViewModels;
using TacosApp.Web.Services;
using Newtonsoft.Json;

namespace TacosApp.Web.Controllers
{
    public class OrderController : Controller
    {
        private readonly TacosDbContext _db;

        public OrderController()
        {
            _db = new TacosDbContext();
        }

        private CartService CartSvc
        {
            get { return new CartService(this.Session); }
        }

        private OrderService OrderSvc
        {
            get { return new OrderService(_db); }
        }

        // GET /Order/Cart
        public ActionResult Cart()
        {
            CartViewModel cart = this.CartSvc.GetCart();
            return View(cart);
        }

        // POST /Order/AddToCart (AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult AddToCart(int menuId, string menuName, decimal menuPrice,
            int quantity, string toppingsJson)
        {
            if (quantity < 1)
            {
                quantity = 1;
            }

            List<CartToppingViewModel> toppings = new List<CartToppingViewModel>();
            if (!string.IsNullOrEmpty(toppingsJson))
            {
                try
                {
                    toppings = JsonConvert.DeserializeObject<List<CartToppingViewModel>>(toppingsJson);
                }
                catch
                {
                    toppings = new List<CartToppingViewModel>();
                }
            }

            // ItemKey = menuId + ソート済みtoppingIds でカートの同一アイテムを判定
            List<int> toppingIdList = new List<int>();
            foreach (CartToppingViewModel t in toppings)
            {
                toppingIdList.Add(t.ToppingId);
            }
            toppingIdList.Sort();

            string[] toppingIdStrings = new string[toppingIdList.Count];
            for (int i = 0; i < toppingIdList.Count; i++)
            {
                toppingIdStrings[i] = toppingIdList[i].ToString();
            }
            string itemKey = string.Format("{0}_{1}", menuId, string.Join("_", toppingIdStrings));

            CartItemViewModel item = new CartItemViewModel
            {
                ItemKey = itemKey,
                MenuId = menuId,
                MenuName = menuName,
                MenuPrice = menuPrice,
                Quantity = quantity,
                Toppings = toppings
            };

            this.CartSvc.AddItem(item);
            CartViewModel cart = this.CartSvc.GetCart();

            return Json(new { success = true, itemCount = cart.ItemCount, total = cart.Total });
        }

        // POST /Order/RemoveFromCart (AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult RemoveFromCart(string itemKey)
        {
            this.CartSvc.RemoveItem(itemKey);
            CartViewModel cart = this.CartSvc.GetCart();
            return Json(new { success = true, itemCount = cart.ItemCount, total = cart.Total });
        }

        // POST /Order/UpdateQuantity (AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult UpdateQuantity(string itemKey, int quantity)
        {
            this.CartSvc.UpdateQuantity(itemKey, quantity);
            CartViewModel cart = this.CartSvc.GetCart();
            return Json(new { success = true, itemCount = cart.ItemCount, total = cart.Total });
        }

        // GET /Order/Checkout
        public ActionResult Checkout()
        {
            CartViewModel cart = this.CartSvc.GetCart();
            if (cart.Items.Count == 0)
            {
                return RedirectToAction("Cart");
            }

            return View(new CheckoutViewModel());
        }

        // POST /Order/Checkout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Checkout(CheckoutViewModel model)
        {
            CartViewModel cart = this.CartSvc.GetCart();
            if (cart.Items.Count == 0)
            {
                return RedirectToAction("Cart");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // セッションに一時保存して確認画面へ
            Session["CheckoutInfo"] = JsonConvert.SerializeObject(model);
            return RedirectToAction("Confirm");
        }

        // GET /Order/Confirm
        public ActionResult Confirm()
        {
            CartViewModel cart = this.CartSvc.GetCart();
            string infoJson = Session["CheckoutInfo"] as string;
            if (cart.Items.Count == 0 || string.IsNullOrEmpty(infoJson))
            {
                return RedirectToAction("Cart");
            }

            CheckoutViewModel info = JsonConvert.DeserializeObject<CheckoutViewModel>(infoJson);
            OrderConfirmViewModel vm = new OrderConfirmViewModel
            {
                CustomerName = info.CustomerName,
                Phone = info.Phone,
                DeliveryAddress = info.DeliveryAddress,
                DeliveryNote = info.DeliveryNote,
                Items = cart.Items,
                Total = cart.Total
            };

            return View(vm);
        }

        // POST /Order/PlaceOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PlaceOrder()
        {
            CartViewModel cart = this.CartSvc.GetCart();
            string infoJson = Session["CheckoutInfo"] as string;
            if (cart.Items.Count == 0 || string.IsNullOrEmpty(infoJson))
            {
                return RedirectToAction("Cart");
            }

            CheckoutViewModel info = JsonConvert.DeserializeObject<CheckoutViewModel>(infoJson);
            TacosApp.Web.Models.Domain.Order order = this.OrderSvc.CreateOrder(cart, info.CustomerName, info.Phone,
                info.DeliveryAddress, info.DeliveryNote);

            this.CartSvc.Clear();
            Session.Remove("CheckoutInfo");

            return RedirectToAction("Complete", new { orderNumber = order.OrderNumber });
        }

        // GET /Order/Complete
        public ActionResult Complete(string orderNumber)
        {
            if (string.IsNullOrEmpty(orderNumber))
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.OrderNumber = orderNumber;
            return View();
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
