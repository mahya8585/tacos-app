using Microsoft.AspNetCore.Mvc.RazorPages;
using TacosApp.Admin.Models;
using TacosApp.Admin.Services;

namespace TacosApp.Admin.Pages.Toppings;

public class IndexModel(IToppingService toppingService) : PageModel
{
    public IReadOnlyList<Topping> Toppings { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken ct)
    {
        Toppings = await toppingService.GetAllAsync(ct);
    }
}
