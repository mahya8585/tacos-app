using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using TacosApp.Web.Data;
using TacosApp.Web.Models.Domain;
using TacosApp.Web.Models.ViewModels;

namespace TacosApp.Web.Services
{
    public class OrderService
    {
        private readonly TacosDbContext _db;

        public OrderService(TacosDbContext db)
        {
            _db = db;
        }

        /// <summary>カートと配達情報から注文を作成する</summary>
        public Order CreateOrder(CartViewModel cart, string customerName, string phone,
            string deliveryAddress, string deliveryNote)
        {
            string orderNumber = GenerateOrderNumber();

            Order order = new Order
            {
                OrderNumber = orderNumber,
                CustomerName = customerName,
                Phone = phone,
                DeliveryAddress = deliveryAddress,
                DeliveryNote = deliveryNote,
                TotalAmount = cart.Total,
                Status = OrderStatus.Received,
                OrderedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                Items = new List<OrderItem>()
            };

            foreach (CartItemViewModel cartItem in cart.Items)
            {
                OrderItem orderItem = new OrderItem
                {
                    MenuId = cartItem.MenuId,
                    Quantity = cartItem.Quantity,
                    UnitPrice = cartItem.MenuPrice,
                    Toppings = new List<OrderItemTopping>()
                };

                foreach (CartToppingViewModel topping in cartItem.Toppings)
                {
                    OrderItemTopping orderTopping = new OrderItemTopping
                    {
                        ToppingId = topping.ToppingId,
                        UnitPrice = topping.Price
                    };
                    orderItem.Toppings.Add(orderTopping);
                }

                order.Items.Add(orderItem);
            }

            _db.Orders.Add(order);
            _db.SaveChanges();
            return order;
        }

        public Order GetByOrderNumber(string orderNumber)
        {
            IQueryable<Order> query = from o in _db.Orders
                                          .Include("Items.Toppings.Topping")
                                          .Include("Items.Menu")
                                      where o.OrderNumber == orderNumber
                                      select o;
            return query.FirstOrDefault();
        }

        public Order GetById(int orderId)
        {
            IQueryable<Order> query = from o in _db.Orders
                                          .Include("Items.Toppings.Topping")
                                          .Include("Items.Menu")
                                      where o.OrderId == orderId
                                      select o;
            return query.FirstOrDefault();
        }

        public List<Order> GetAll()
        {
            IQueryable<Order> query = from o in _db.Orders
                                          .Include("Items.Toppings.Topping")
                                          .Include("Items.Menu")
                                      orderby o.OrderedAt descending
                                      select o;
            return query.ToList();
        }

        public Order UpdateStatus(int orderId, OrderStatus newStatus)
        {
            Order order = _db.Orders.Find(orderId);
            if (order == null)
            {
                return null;
            }

            order.Status = newStatus;
            order.UpdatedAt = DateTime.Now;
            _db.SaveChanges();
            return order;
        }

        private string GenerateOrderNumber()
        {
            string datePart = DateTime.Now.ToString("yyMMdd");
            // 同日の最大シーケンス番号を取得
            string prefix = "TACOS-" + datePart + "-";

            IQueryable<string> sameDayNumbersQuery = from o in _db.Orders
                                                     where o.OrderNumber.StartsWith(prefix)
                                                     select o.OrderNumber;
            List<string> sameDayNumbers = sameDayNumbersQuery.ToList();

            int maxSeq = 0;
            foreach (string n in sameDayNumbers)
            {
                string[] parts = n.Split('-');
                int seq;
                if (parts.Length == 3 && int.TryParse(parts[2], out seq))
                {
                    if (seq > maxSeq)
                    {
                        maxSeq = seq;
                    }
                }
            }

            return prefix + (maxSeq + 1).ToString("D6");
        }
    }
}
