using Microsoft.AspNetCore.Mvc.RazorPages;
using TacosApp.Admin.Models;
using TacosApp.Admin.Services;

namespace TacosApp.Admin.Pages;

public class IndexModel(IOrderService orderService) : PageModel
{
    public OrderStatsDto Stats { get; private set; } = new(0, 0, 0, 0, 0, 0m);
    public IReadOnlyList<Order> RecentOrders { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken ct)
    {
        Stats = await orderService.GetStatsAsync(ct);
        var all = await orderService.GetAllAsync(ct: ct);
        RecentOrders = [.. all.Take(10)];
    }
}
