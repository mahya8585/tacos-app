using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TacosApp.Admin.Models;
using TacosApp.Admin.Services;

namespace TacosApp.Admin.Pages.Menus;

public class EditModel(IMenuService menuService) : PageModel
{
    [BindProperty]
    public MenuEditInput Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken ct)
    {
        var menu = await menuService.GetByIdAsync(id, ct);
        if (menu is null) return NotFound();

        Input = new MenuEditInput
        {
            MenuId = menu.MenuId,
            Name = menu.Name,
            Description = menu.Description,
            Price = menu.Price,
            ImageUrl = menu.ImageUrl,
            IsAvailable = menu.IsAvailable,
            DisplayOrder = menu.DisplayOrder
        };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var ok = await menuService.UpdateAsync(new Menu
        {
            MenuId = Input.MenuId,
            Name = Input.Name,
            Description = Input.Description,
            Price = Input.Price,
            ImageUrl = Input.ImageUrl,
            IsAvailable = Input.IsAvailable,
            DisplayOrder = Input.DisplayOrder
        }, ct);

        if (!ok) return NotFound();
        return RedirectToPage("Index");
    }

    public sealed class MenuEditInput
    {
        public int MenuId { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Range(0, 1_000_000)]
        public decimal Price { get; set; }

        [StringLength(500), Url]
        public string? ImageUrl { get; set; }

        public bool IsAvailable { get; set; }

        public int DisplayOrder { get; set; }
    }
}
