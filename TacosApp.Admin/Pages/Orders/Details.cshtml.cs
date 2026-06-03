using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TacosApp.Admin.Models;
using TacosApp.Admin.Services;

namespace TacosApp.Admin.Pages.Orders;

public class DetailsModel(IOrderService orderService) : PageModel
{
    public Order? Order { get; private set; }

    [BindProperty]
    public OrderStatus NewStatus { get; set; }

    public async Task OnGetAsync(int id, CancellationToken ct)
    {
        Order = await orderService.GetByIdAsync(id, ct);
        if (Order is not null)
        {
            NewStatus = Order.Status;
        }
    }

    public async Task<IActionResult> OnPostAsync(int id, CancellationToken ct)
    {
        var ok = await orderService.UpdateStatusAsync(id, NewStatus, ct);
        if (!ok)
        {
            return NotFound();
        }
        return RedirectToPage(new { id });
    }
}
