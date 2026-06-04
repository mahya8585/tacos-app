using System.Net.Http.Json;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using TacosApp.Admin.Data;
using TacosApp.Admin.Hubs;
using TacosApp.Admin.Models;

namespace TacosApp.Admin.Services;

public sealed class OrderService(
    AdminDbContext db,
    IHubContext<AdminOrderHub> hub,
    IHttpClientFactory httpClientFactory,
    ILogger<OrderService> logger) : IOrderService
{
    public async Task<IReadOnlyList<Order>> GetAllAsync(OrderStatus? filter = null, CancellationToken ct = default)
    {
        IQueryable<Order> q = db.Orders
            .AsNoTracking()
            .Include(o => o.Items).ThenInclude(i => i.Menu)
            .Include(o => o.Items).ThenInclude(i => i.Toppings).ThenInclude(t => t.Topping)
            .OrderByDescending(o => o.OrderedAt);

        if (filter is { } status)
        {
            q = q.Where(o => o.Status == status);
        }

        return await q.ToListAsync(ct);
    }

    public Task<Order?> GetByIdAsync(int orderId, CancellationToken ct = default) =>
        db.Orders
            .AsNoTracking()
            .Include(o => o.Items).ThenInclude(i => i.Menu)
            .Include(o => o.Items).ThenInclude(i => i.Toppings).ThenInclude(t => t.Topping)
            .FirstOrDefaultAsync(o => o.OrderId == orderId, ct);

    public async Task<bool> UpdateStatusAsync(int orderId, OrderStatus newStatus, CancellationToken ct = default)
    {
        var order = await db.Orders.FirstOrDefaultAsync(o => o.OrderId == orderId, ct);
        if (order is null)
        {
            return false;
        }

        order.Status = newStatus;
        order.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Order {OrderId} status updated to {Status}", orderId, newStatus);

        await hub.Clients.All.SendAsync("orderStatusChanged", new
        {
            order.OrderId,
            order.OrderNumber,
            Status = (int)order.Status,
            StatusLabel = order.Status.ToString(),
            UpdatedAt = order.UpdatedAt
        }, ct);

        // TacosApp.Web の OrderStatusNotificationService を通じて顧客ブラウザに通知
        try
        {
            var webClient = httpClientFactory.CreateClient("TacosWeb");
            await webClient.PutAsJsonAsync(
                $"api/orders/{orderId}/status",
                new { Status = (int)newStatus },
                ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to notify TacosApp.Web of status change for order {OrderId}", orderId);
        }

        return true;
    }

    public async Task<OrderStatsDto> GetStatsAsync(CancellationToken ct = default)
    {
        var grouped = await db.Orders
            .AsNoTracking()
            .GroupBy(o => o.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        var revenue = await db.Orders
            .AsNoTracking()
            .Where(o => o.Status == OrderStatus.Delivered)
            .SumAsync(o => (decimal?)o.TotalAmount, ct) ?? 0m;

        int CountFor(OrderStatus s) => grouped.FirstOrDefault(x => x.Status == s)?.Count ?? 0;

        return new OrderStatsDto(
            Total: grouped.Sum(x => x.Count),
            Received: CountFor(OrderStatus.Received),
            Preparing: CountFor(OrderStatus.Preparing),
            OnDelivery: CountFor(OrderStatus.OnDelivery),
            Delivered: CountFor(OrderStatus.Delivered),
            TotalRevenue: revenue);
    }
}
