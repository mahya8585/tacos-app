using TacosApp.Admin.Models;

namespace TacosApp.Admin.Services;

public interface IOrderService
{
    Task<IReadOnlyList<Order>> GetAllAsync(OrderStatus? filter = null, CancellationToken ct = default);
    Task<Order?> GetByIdAsync(int orderId, CancellationToken ct = default);
    Task<bool> UpdateStatusAsync(int orderId, OrderStatus newStatus, CancellationToken ct = default);
    Task<OrderStatsDto> GetStatsAsync(CancellationToken ct = default);
}

public sealed record OrderStatsDto(int Total, int Received, int Preparing, int OnDelivery, int Delivered, decimal TotalRevenue);
