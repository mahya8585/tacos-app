using Microsoft.AspNetCore.Mvc.RazorPages;
using TacosApp.Admin.Models;
using TacosApp.Admin.Services;

namespace TacosApp.Admin.Pages.Orders;

public class IndexModel(IOrderService orderService) : PageModel
{
    public IReadOnlyList<Order> Orders { get; private set; } = [];

    [Microsoft.AspNetCore.Mvc.BindProperty(SupportsGet = true)]
    public OrderStatus? Filter { get; set; }

    public async Task OnGetAsync(CancellationToken ct)
    {
        Orders = await orderService.GetAllAsync(Filter, ct);
    }
}
