using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TacosApp.Admin.Models;
using TacosApp.Admin.Services;

namespace TacosApp.Admin.Pages.Toppings;

public class EditModel(IToppingService toppingService) : PageModel
{
    [BindProperty]
    public ToppingEditInput Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken ct)
    {
        var topping = await toppingService.GetByIdAsync(id, ct);
        if (topping is null) return NotFound();

        Input = new ToppingEditInput
        {
            ToppingId = topping.ToppingId,
            Name = topping.Name,
            Price = topping.Price,
            IsAvailable = topping.IsAvailable,
            DisplayOrder = topping.DisplayOrder
        };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var ok = await toppingService.UpdateAsync(new Topping
        {
            ToppingId = Input.ToppingId,
            Name = Input.Name,
            Price = Input.Price,
            IsAvailable = Input.IsAvailable,
            DisplayOrder = Input.DisplayOrder
        }, ct);

        if (!ok) return NotFound();
        return RedirectToPage("Index");
    }

    public sealed class ToppingEditInput
    {
        public int ToppingId { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Range(0, 100_000)]
        public decimal Price { get; set; }

        public bool IsAvailable { get; set; }

        public int DisplayOrder { get; set; }
    }
}
