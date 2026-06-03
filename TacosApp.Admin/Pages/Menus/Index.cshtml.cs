using Microsoft.AspNetCore.Mvc.RazorPages;
using TacosApp.Admin.Models;
using TacosApp.Admin.Services;

namespace TacosApp.Admin.Pages.Menus;

public class IndexModel(IMenuService menuService) : PageModel
{
    public IReadOnlyList<Menu> Menus { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken ct)
    {
        Menus = await menuService.GetAllAsync(ct);
    }
}
