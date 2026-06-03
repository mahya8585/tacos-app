using System.Linq;
using System.Web.Mvc;
using TacosApp.Web.Data;
using TacosApp.Web.Models.Domain;

namespace TacosApp.Web.Controllers
{
    public class StatusController : Controller
    {
        private readonly TacosDbContext _db;

        public StatusController()
        {
            _db = new TacosDbContext();
        }

        // GET /Status/Index?orderNumber=TACOS-xxxxxx-000001
        public ActionResult Index(string orderNumber)
        {
            if (string.IsNullOrWhiteSpace(orderNumber))
            {
                return RedirectToAction("Index", "Home");
            }

            IQueryable<Order> query = from o in _db.Orders
                                      where o.OrderNumber == orderNumber
                                      select o;
            Order order = query.FirstOrDefault();
            if (order == null)
            {
                ViewBag.Error = "ご注文が見つかりませんでした。注文番号をご確認ください。";
                return View("NotFound");
            }

            ViewBag.OrderNumber = order.OrderNumber;
            ViewBag.CustomerName = order.CustomerName;
            ViewBag.CurrentStatus = (int)order.Status;
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
