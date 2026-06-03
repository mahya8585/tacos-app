using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TacosApp.Admin.Models;
using TacosApp.Admin.Services;

namespace TacosApp.Admin.Pages.Toppings;

public class CreateModel(IToppingService toppingService) : PageModel
{
    [BindProperty]
    public ToppingInput Input { get; set; } = new();

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        await toppingService.CreateAsync(new Topping
        {
            Name = Input.Name,
            Price = Input.Price,
            IsAvailable = Input.IsAvailable,
            DisplayOrder = Input.DisplayOrder
        }, ct);

        return RedirectToPage("Index");
    }

    public sealed class ToppingInput
    {
        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Range(0, 100_000)]
        public decimal Price { get; set; }

        public bool IsAvailable { get; set; } = true;

        public int DisplayOrder { get; set; }
    }
}
