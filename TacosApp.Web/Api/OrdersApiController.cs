using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Microsoft.AspNet.SignalR;
using TacosApp.Web.Data;
using TacosApp.Web.Filters;
using TacosApp.Web.Hubs;
using TacosApp.Web.Models.Api;
using TacosApp.Web.Models.Domain;

namespace TacosApp.Web.Api
{
    [ApiKeyAuthFilter]
    [RoutePrefix("api/orders")]
    public class OrdersApiController : ApiController
    {
        private readonly TacosDbContext _db;

        public OrdersApiController()
        {
            _db = new TacosDbContext();
        }

        // GET api/orders
        [HttpGet, Route("")]
        public IHttpActionResult GetOrders()
        {
            IQueryable<Order> query = from o in _db.Orders
                                          .Include("Items.Menu")
                                          .Include("Items.Toppings.Topping")
                                      orderby o.OrderedAt descending
                                      select o;
            List<Order> orders = query.ToList();

            List<OrderDto> dtoList = new List<OrderDto>();
            foreach (Order order in orders)
            {
                dtoList.Add(MapToDto(order));
            }
            return Ok(dtoList);
        }

        // GET api/orders/{id}
        [HttpGet, Route("{id:int}")]
        public IHttpActionResult GetOrder(int id)
        {
            IQueryable<Order> query = from o in _db.Orders
                                          .Include("Items.Menu")
                                          .Include("Items.Toppings.Topping")
                                      where o.OrderId == id
                                      select o;
            Order order = query.FirstOrDefault();

            if (order == null)
            {
                return NotFound();
            }

            return Ok(MapToDto(order));
        }

        // PUT api/orders/{id}/status
        [HttpPut, Route("{id:int}/status")]
        public IHttpActionResult UpdateStatus(int id, [FromBody] UpdateStatusRequest request)
        {
            if (request == null)
            {
                return BadRequest("リクエストボディが必要です。");
            }

            if (request.Status < 0 || request.Status > 3)
            {
                return BadRequest("ステータス値は 0〜3 の範囲で指定してください。");
            }

            Order order = _db.Orders.Find(id);
            if (order == null)
            {
                return NotFound();
            }

            OrderStatus newStatus = (OrderStatus)request.Status;
            order.Status = newStatus;
            order.UpdatedAt = DateTime.Now;
            _db.SaveChanges();

            // SignalR でブラウザに即時通知
            IHubContext hubContext = GlobalHost.ConnectionManager.GetHubContext<OrderStatusHub>();
            hubContext.Clients.Group(order.OrderNumber).statusUpdated(request.Status, GetStatusLabel(newStatus));

            return Ok(MapToDto(order));
        }

        private static OrderDto MapToDto(Order order)
        {
            OrderDto dto = new OrderDto
            {
                OrderId = order.OrderId,
                OrderNumber = order.OrderNumber,
                CustomerName = order.CustomerName,
                Phone = order.Phone,
                DeliveryAddress = order.DeliveryAddress,
                DeliveryNote = order.DeliveryNote,
                TotalAmount = order.TotalAmount,
                Status = (int)order.Status,
                StatusLabel = GetStatusLabel(order.Status),
                OrderedAt = order.OrderedAt,
                UpdatedAt = order.UpdatedAt,
                Items = new List<OrderItemDto>()
            };

            foreach (OrderItem i in order.Items)
            {
                OrderItemDto itemDto = new OrderItemDto
                {
                    OrderItemId = i.OrderItemId,
                    MenuName = (i.Menu != null) ? i.Menu.Name : "",
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    Toppings = new List<OrderItemToppingDto>()
                };

                foreach (OrderItemTopping t in i.Toppings)
                {
                    OrderItemToppingDto toppingDto = new OrderItemToppingDto
                    {
                        ToppingName = (t.Topping != null) ? t.Topping.Name : "",
                        UnitPrice = t.UnitPrice
                    };
                    itemDto.Toppings.Add(toppingDto);
                }

                dto.Items.Add(itemDto);
            }

            return dto;
        }

        private static string GetStatusLabel(OrderStatus status)
        {
            switch (status)
            {
                case OrderStatus.Received: return "注文受付";
                case OrderStatus.Preparing: return "調理中";
                case OrderStatus.OnDelivery: return "配達中";
                case OrderStatus.Delivered: return "配達完了";
                default: return "不明";
            }
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
