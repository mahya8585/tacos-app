using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TacosApp.Admin.Models;
using TacosApp.Admin.Services;

namespace TacosApp.Admin.Pages.Menus;

public class CreateModel(IMenuService menuService) : PageModel
{
    [BindProperty]
    public MenuInputModel Input { get; set; } = new();

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        await menuService.CreateAsync(new Menu
        {
            Name = Input.Name,
            Description = Input.Description,
            Price = Input.Price,
            ImageUrl = Input.ImageUrl,
            IsAvailable = Input.IsAvailable,
            DisplayOrder = Input.DisplayOrder
        }, ct);

        return RedirectToPage("Index");
    }

    public sealed class MenuInputModel
    {
        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Range(0, 1_000_000)]
        public decimal Price { get; set; }

        [StringLength(500), Url]
        public string? ImageUrl { get; set; }

        public bool IsAvailable { get; set; } = true;

        public int DisplayOrder { get; set; }
    }
}
