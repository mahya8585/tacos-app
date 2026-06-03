using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TacosApp.Admin.Models;
using TacosApp.Admin.Services;

namespace TacosApp.Admin.Pages.Toppings;

public class DeleteModel(IToppingService toppingService) : PageModel
{
    [BindProperty]
    public Topping? Topping { get; set; }

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken ct)
    {
        Topping = await toppingService.GetByIdAsync(id, ct);
        if (Topping is null) return NotFound();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id, CancellationToken ct)
    {
        await toppingService.DeleteAsync(id, ct);
        return RedirectToPage("Index");
    }
}
