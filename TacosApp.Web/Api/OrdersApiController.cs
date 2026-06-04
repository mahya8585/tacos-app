using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using TacosApp.Web.Data;
using TacosApp.Web.Filters;
using TacosApp.Web.Hubs;
using TacosApp.Web.Models.Api;
using TacosApp.Web.Models.Domain;

namespace TacosApp.Web.Api
{
    [ServiceFilter(typeof(ApiKeyAuthFilter))]
    [ApiController]
    [Route("api/orders")]
    public class OrdersApiController : ControllerBase
    {
        private readonly TacosDbContext _db;
        private readonly IHubContext<OrderStatusHub> _hubContext;

        public OrdersApiController(TacosDbContext db, IHubContext<OrderStatusHub> hubContext)
        {
            _db = db;
            _hubContext = hubContext;
        }

        // GET api/orders
        [HttpGet("")]
        public IActionResult GetOrders()
        {
            List<Order> orders = _db.Orders
                .Include(o => o.Items).ThenInclude(i => i.Menu)
                .Include(o => o.Items).ThenInclude(i => i.Toppings).ThenInclude(t => t.Topping)
                .OrderByDescending(o => o.OrderedAt)
                .ToList();

            List<OrderDto> dtoList = new List<OrderDto>();
            foreach (Order order in orders)
            {
                dtoList.Add(MapToDto(order));
            }
            return Ok(dtoList);
        }

        // GET api/orders/{id}
        [HttpGet("{id:int}")]
        public IActionResult GetOrder(int id)
        {
            Order? order = _db.Orders
                .Include(o => o.Items).ThenInclude(i => i.Menu)
                .Include(o => o.Items).ThenInclude(i => i.Toppings).ThenInclude(t => t.Topping)
                .FirstOrDefault(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            return Ok(MapToDto(order));
        }

        // PUT api/orders/{id}/status
        [HttpPut("{id:int}/status")]
        public IActionResult UpdateStatus(int id, [FromBody] UpdateStatusRequest request)
        {
            if (request == null)
            {
                return BadRequest("リクエストボディが必要です。");
            }

            if (request.Status < 0 || request.Status > 3)
            {
                return BadRequest("ステータス値は 0〜3 の範囲で指定してください。");
            }

            Order? order = _db.Orders.Find(id);
            if (order == null)
            {
                return NotFound();
            }

            OrderStatus newStatus = (OrderStatus)request.Status;
            order.Status = newStatus;
            order.UpdatedAt = DateTime.Now;
            _db.SaveChanges();

            // SignalR でブラウザに即時通知
            _hubContext.Clients.Group(order.OrderNumber)
                .SendAsync("statusUpdated", request.Status, GetStatusLabel(newStatus));

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

    }
}
