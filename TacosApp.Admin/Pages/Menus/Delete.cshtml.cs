using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TacosApp.Admin.Models;
using TacosApp.Admin.Services;

namespace TacosApp.Admin.Pages.Menus;

public class DeleteModel(IMenuService menuService) : PageModel
{
    [BindProperty]
    public Menu? Menu { get; set; }

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken ct)
    {
        Menu = await menuService.GetByIdAsync(id, ct);
        if (Menu is null) return NotFound();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id, CancellationToken ct)
    {
        await menuService.DeleteAsync(id, ct);
        return RedirectToPage("Index");
    }
}
